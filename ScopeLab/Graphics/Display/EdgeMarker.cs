using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osuTK;
using ScopeLab.Simulation;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// A draggable marker sitting on the edge of the screen, which glides to follow the setting it represents.
/// </summary>
/// <remarks>
/// The marker must be placed in a track that spans the screen along the marker's axis.
/// </remarks>
public abstract partial class EdgeMarker : CompositeDrawable, IHasTooltip
{
    private const double guide_linger_duration = 1200;

    public LocalisableString TooltipText { get; init; }

    /// <summary>
    /// A line drawn across the screen while the marker is being adjusted.
    /// </summary>
    public GuideLine? Guide { get; init; }

    [Resolved]
    protected Oscilloscope Scope { get; private set; } = null!;

    private readonly Direction axis;
    private readonly MarkerTag tag;

    private float lastTarget = float.NaN;
    private double lastChangeTime = double.NegativeInfinity;

    protected EdgeMarker(Direction axis, MarkerTag tag)
    {
        this.axis = axis;
        this.tag = tag;

        AutoSizeAxes = Axes.Both;
        AlwaysPresent = true;
        InternalChild = tag;
        Alpha = 0;
    }

    /// <summary>
    /// Offset across the track, used to set coinciding markers side by side.
    /// </summary>
    public float CrossOffset { get; set; }

    /// <summary>
    /// The marker's current position along its track, in pixels.
    /// </summary>
    public float TrackPosition { get; private set; } = float.NaN;

    public bool IsShown => Available;

    /// <summary>
    /// Pixels per division on the screen.
    /// </summary>
    protected float Division => axis == Direction.Vertical
        ? TrackLength / Oscilloscope.VERTICAL_DIVISIONS
        : TrackLength / Oscilloscope.HORIZONTAL_DIVISIONS;

    protected float TrackLength => axis == Direction.Vertical ? Parent!.DrawHeight : Parent!.DrawWidth;

    /// <summary>
    /// Where the marker should be along the track, in pixels.
    /// </summary>
    protected abstract float TargetPosition { get; }

    protected virtual bool Available => Scope.DisplayMode.Value == DisplayMode.YT;

    /// <summary>
    /// Applies a new setting from a drag to the given track position.
    /// </summary>
    protected abstract void DragTo(float trackPosition);

    protected override void Update()
    {
        base.Update();

        float target = TargetPosition;

        if (target != lastTarget)
        {
            if (!float.IsNaN(lastTarget))
                lastChangeTime = Time.Current;

            lastTarget = target;
        }

        float clamped = Math.Clamp(target, 0, TrackLength);

        TrackPosition = float.IsNaN(TrackPosition) ? clamped : (float)Interpolation.DampContinuously(TrackPosition, clamped, 30, Time.Elapsed);

        if (axis == Direction.Vertical)
        {
            Y = TrackPosition;
            X = (float)Interpolation.DampContinuously(X, CrossOffset, 30, Time.Elapsed);
        }
        else
        {
            X = TrackPosition;
            Y = (float)Interpolation.DampContinuously(Y, CrossOffset, 30, Time.Elapsed);
        }

        bool inRange = Math.Abs(target - clamped) < 0.5f;
        float targetAlpha = Available ? inRange ? 1 : 0.45f : 0;
        Alpha = (float)Interpolation.DampContinuously(Alpha, targetAlpha, 40, Time.Elapsed);

        if (Guide == null)
            return;

        bool showGuide = Available && (IsHovered || IsDragged || Time.Current - lastChangeTime < guide_linger_duration);

        if (axis == Direction.Vertical)
            Guide.Y = TrackPosition;
        else
            Guide.X = TrackPosition;

        Guide.Alpha = (float)Interpolation.DampContinuously(Guide.Alpha, showGuide ? 0.75f : 0, 50, Time.Elapsed);
    }

    public override bool HandlePositionalInput => Available && base.HandlePositionalInput;

    protected override bool OnHover(HoverEvent e)
    {
        tag.ScaleTo(1.2f, 300, Easing.OutQuint);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        if (!IsDragged)
            tag.ScaleTo(1, 300, Easing.OutQuint);
    }

    protected override bool OnDragStart(DragStartEvent e) => true;

    protected override void OnDrag(DragEvent e)
    {
        Vector2 local = Parent!.ToLocalSpace(e.ScreenSpaceMousePosition);
        DragTo(Math.Clamp(axis == Direction.Vertical ? local.Y : local.X, 0, TrackLength));
    }

    protected override void OnDragEnd(DragEndEvent e)
    {
        if (!IsHovered)
            tag.ScaleTo(1, 300, Easing.OutQuint);
    }

    protected override bool OnDoubleClick(DoubleClickEvent e)
    {
        ResetToDefault();
        return true;
    }

    protected abstract void ResetToDefault();
}

/// <summary>
/// Marks the ground reference of a channel on the left edge of the screen.
/// </summary>
public partial class ChannelMarker(ScopeChannel channel) : EdgeMarker(Direction.Vertical, new MarkerTag($"{channel.Index + 1}", ScopeColours.ForChannel(channel.Index), MarkerDirection.Right))
{
    protected override float TargetPosition => TrackLength / 2 - (float)channel.Position.Value * Division;

    protected override bool Available => base.Available && channel.Enabled.Value;

    protected override void DragTo(float trackPosition) => channel.Position.Value = Math.Round((TrackLength / 2 - trackPosition) / Division, 2);

    protected override void ResetToDefault() => channel.Position.SetDefault();
}

/// <summary>
/// Marks the trigger threshold on the right edge of the screen.
/// </summary>
public partial class TriggerLevelMarker() : EdgeMarker(Direction.Vertical, new MarkerTag("T", ScopeColours.TRIGGER, MarkerDirection.Left))
{
    protected override float TargetPosition => TrackLength / 2 - (float)Scope.TriggerChannel.ToDivisions(Scope.Trigger.Level.Value) * Division;

    protected override bool Available => base.Available && !Scope.IsRolling;

    protected override void DragTo(float trackPosition)
    {
        var channel = Scope.TriggerChannel;
        double volts = channel.ToVolts((TrackLength / 2 - trackPosition) / Division);

        // Snap to a resolution of 1/50 of a division, which keeps the readout tidy.
        double resolution = channel.VoltsPerDivision.Value / 50;
        Scope.Trigger.Level.Value = Math.Round(volts / resolution) * resolution;
    }

    protected override void ResetToDefault() => Scope.Trigger.Level.Value = 0;
}

/// <summary>
/// Marks where the trigger point sits horizontally, along the top edge of the screen.
/// </summary>
public partial class TriggerPositionMarker() : EdgeMarker(Direction.Horizontal, new MarkerTag("T", ScopeColours.TRIGGER, MarkerDirection.Down))
{
    protected override float TargetPosition => (float)(Oscilloscope.HORIZONTAL_DIVISIONS / 2.0 + Scope.Timebase.Position.Value) * Division;

    protected override bool Available => base.Available && !Scope.IsRolling;

    protected override void DragTo(float trackPosition) =>
        Scope.Timebase.Position.Value = Math.Round(trackPosition / Division - Oscilloscope.HORIZONTAL_DIVISIONS / 2.0, 2);

    protected override void ResetToDefault() => Scope.Timebase.Position.SetDefault();
}
