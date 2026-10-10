using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osuTK;
using ScopeLab.Simulation;
using ScopeLab.Utils;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// The oscilloscope's screen, including its on-screen readouts and draggable markers.
/// </summary>
public partial class ScopeDisplay : CompositeDrawable
{
    public const float DIVISION = 52;

    private const float rail_width = 28;
    private const float top_bar_height = 30;
    private const float position_rail_height = 22;
    private const float bottom_bar_height = 30;
    private const float bezel_padding = 10;

    private static readonly Vector2 screen_size = new Vector2(DIVISION * Oscilloscope.HORIZONTAL_DIVISIONS, DIVISION * Oscilloscope.VERTICAL_DIVISIONS);

    [Resolved]
    private Oscilloscope scope { get; set; } = null!;

    /// <summary>
    /// How far a channel marker moves aside when it would otherwise cover another one.
    /// </summary>
    private const float marker_stack_offset = 20;

    private readonly ChannelMarker[] channelMarkers = new ChannelMarker[Oscilloscope.CHANNEL_COUNT];

    private Container screenContent = null!;
    private Box powerFlash = null!;

    private StatusBadge runBadge = null!;
    private StatusBadge stateBadge = null!;
    private StatusBadge timebaseBadge = null!;
    private StatusBadge delayBadge = null!;
    private StatusBadge sampleRateBadge = null!;
    private StatusBadge triggerBadge = null!;

    private readonly StatusBadge[] channelBadges = new StatusBadge[Oscilloscope.CHANNEL_COUNT];
    private StatusBadge mathBadge = null!;
    private StatusBadge xyBadge = null!;
    private StatusBadge persistenceBadge = null!;

    public ScopeDisplay()
    {
        Size = new Vector2(screen_size.X + rail_width * 2, top_bar_height + position_rail_height + screen_size.Y + bottom_bar_height) + new Vector2(bezel_padding * 2);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        var guides = new Container { RelativeSizeAxes = Axes.Both };
        var cursor = new CursorReadout();
        var leftRail = new Container { Size = new Vector2(rail_width, screen_size.Y), Y = top_bar_height + position_rail_height };
        var rightRail = new Container { Size = new Vector2(rail_width, screen_size.Y), Position = new Vector2(rail_width + screen_size.X, top_bar_height + position_rail_height) };
        var positionRail = new Container { Size = new Vector2(screen_size.X, position_rail_height), Position = new Vector2(rail_width, top_bar_height) };

        foreach (var channel in scope.Channels)
        {
            var guide = new GuideLine(Direction.Horizontal, ScopeColours.ForChannel(channel.Index));
            guides.Add(guide);
            leftRail.Add(channelMarkers[channel.Index] = new ChannelMarker(channel)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.CentreRight,
                Guide = guide,
                TooltipText = $"{channel.Name} ground level. Drag to move the trace, double-click to reset.",
            });
        }

        var levelGuide = new GuideLine(Direction.Horizontal, ScopeColours.TRIGGER);
        var positionGuide = new GuideLine(Direction.Vertical, ScopeColours.TRIGGER);
        guides.AddRange([levelGuide, positionGuide]);

        rightRail.Add(new TriggerLevelMarker
        {
            Origin = Anchor.CentreLeft,
            Guide = levelGuide,
            TooltipText = "Trigger level. Drag to set the threshold the signal must cross.",
        });

        positionRail.Add(new TriggerPositionMarker
        {
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomCentre,
            Guide = positionGuide,
            TooltipText = "Trigger point. Drag to shift the waveform horizontally.",
        });

        InternalChildren =
        [
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 14,
                BorderThickness = 2,
                BorderColour = ScopeColours.BORDER,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Colour4.Black.Opacity(0.5f),
                    Radius = 12,
                    Offset = new Vector2(0, 4),
                },
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(ScopeColours.PANEL_DARK, ScopeColours.BENCH),
                },
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(bezel_padding),
                Children =
                [
                    createTopBar(),
                    new ScreenArea(cursor)
                    {
                        Position = new Vector2(rail_width, top_bar_height + position_rail_height),
                        Size = screen_size,
                        Children =
                        [
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientVertical(ScopeColours.SCREEN_LIGHT, ScopeColours.SCREEN),
                            },
                            screenContent = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Children = [new Graticule(), new WaveformView(), guides, cursor],
                            },
                            createVignette(),
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Height = 0.45f,
                                Colour = ColourInfo.GradientVertical(Colour4.White.Opacity(0.035f), Colour4.White.Opacity(0)),
                            },
                            powerFlash = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ScopeColours.XY,
                                Alpha = 0,
                                Blending = BlendingParameters.Additive,
                            },
                        ],
                    },
                    leftRail,
                    rightRail,
                    positionRail,
                    createBottomBar(),
                ],
            },
        ];
    }

    private Drawable createTopBar()
    {
        return new Container
        {
            RelativeSizeAxes = Axes.X,
            Height = top_bar_height,
            Padding = new MarginPadding { Horizontal = rail_width },
            Children =
            [
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(6, 0),
                    Children =
                    [
                        runBadge = new StatusBadge(),
                        stateBadge = new StatusBadge(),
                        timebaseBadge = new StatusBadge("H"),
                        delayBadge = new StatusBadge("D"),
                        sampleRateBadge = new StatusBadge("SR"),
                    ],
                },
                triggerBadge = new StatusBadge("T")
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                },
            ],
        };
    }

    private Drawable createBottomBar()
    {
        var flow = new FillFlowContainer
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(6, 0),
        };

        for (int i = 0; i < channelBadges.Length; i++)
        {
            flow.Add(channelBadges[i] = new StatusBadge($"{i + 1}"));
            channelBadges[i].CaptionColour = ScopeColours.ForChannel(i);
        }

        flow.Add(mathBadge = new StatusBadge("M"));
        mathBadge.CaptionColour = ScopeColours.MATH;

        return new Container
        {
            RelativeSizeAxes = Axes.X,
            Height = bottom_bar_height,
            Anchor = Anchor.BottomLeft,
            Origin = Anchor.BottomLeft,
            Padding = new MarginPadding { Horizontal = rail_width },
            Children =
            [
                flow,
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(6, 0),
                    Children =
                    [
                        persistenceBadge = new StatusBadge("PERSIST"),
                        xyBadge = new StatusBadge { Value = "XY" },
                    ],
                },
            ],
        };
    }

    private static Drawable createVignette()
    {
        const float size = 0.12f;
        var shade = Colour4.Black.Opacity(0.45f);
        var clear = Colour4.Black.Opacity(0);

        return new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children =
            [
                new Box { RelativeSizeAxes = Axes.Both, Height = size, Colour = ColourInfo.GradientVertical(shade, clear) },
                new Box { RelativeSizeAxes = Axes.Both, Height = size, Anchor = Anchor.BottomLeft, Origin = Anchor.BottomLeft, Colour = ColourInfo.GradientVertical(clear, shade) },
                new Box { RelativeSizeAxes = Axes.Both, Width = size * 0.8f, Colour = ColourInfo.GradientHorizontal(shade, clear) },
                new Box { RelativeSizeAxes = Axes.Both, Width = size * 0.8f, Anchor = Anchor.TopRight, Origin = Anchor.TopRight, Colour = ColourInfo.GradientHorizontal(clear, shade) },
            ],
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        foreach (var badge in new[] { mathBadge, xyBadge, persistenceBadge })
            badge.Alpha = 0;

        powerOn();
    }

    /// <summary>
    /// The CRT warming up: a bright line that blooms into the full raster.
    /// </summary>
    private void powerOn()
    {
        screenContent.ScaleTo(new Vector2(0.2f, 0.004f))
                     .Delay(250)
                     .ScaleTo(new Vector2(1, 0.004f), 250, Easing.OutQuint)
                     .Then()
                     .ScaleTo(Vector2.One, 650, Easing.OutExpo);

        powerFlash.Delay(500).FadeTo(0.3f, 80).Then().FadeOut(1200, Easing.OutQuint);
    }

    protected override void Update()
    {
        base.Update();

        separateChannelMarkers();
        updateTopBar();
        updateBottomBar();
    }

    /// <summary>
    /// Sets coinciding channel markers side by side, so neither hides the other.
    /// </summary>
    private void separateChannelMarkers()
    {
        var first = channelMarkers[0];
        var second = channelMarkers[1];

        bool overlapping = first.IsShown && second.IsShown && Math.Abs(first.TrackPosition - second.TrackPosition) < first.DrawHeight + 2;

        second.CrossOffset = overlapping ? -marker_stack_offset : 0;
    }

    private void updateTopBar()
    {
        var state = scope.State.Value;

        if (!scope.Running.Value)
        {
            runBadge.Value = "STOP";
            runBadge.SetFill(ScopeColours.STOP, 1);
        }
        else if (scope.SingleArmed.Value)
        {
            runBadge.Value = "SINGLE";
            runBadge.SetFill(ScopeColours.TRIGGER, 1);
        }
        else
        {
            runBadge.Value = "RUN";
            runBadge.SetFill(ScopeColours.RUN, 1);
        }

        runBadge.ValueColour = ScopeColours.SCREEN;

        (stateBadge.Value, stateBadge.ValueColour) = state switch
        {
            AcquisitionState.Triggered => ("TRIG'D", ScopeColours.RUN),
            AcquisitionState.Auto => ("AUTO", ScopeColours.TEXT),
            AcquisitionState.Roll => ("ROLL", ScopeColours.GENERATOR),
            AcquisitionState.Waiting => ("WAIT", ScopeColours.TRIGGER),
            _ => ("HOLD", ScopeColours.TEXT_DIM),
        };

        double secondsPerDivision = scope.Timebase.SecondsPerDivision.Value;

        timebaseBadge.Value = SiFormat.Format(secondsPerDivision, "s") + "/div";
        delayBadge.Value = SiFormat.Format(scope.Timebase.Position.Value * secondsPerDivision, "s", signed: true);
        sampleRateBadge.Value = SiFormat.Format(Acquisition.SAMPLE_COUNT / scope.Timebase.Window, "Sa/s");

        var trigger = scope.Trigger;
        triggerBadge.Icon = trigger.Slope.Value == TriggerSlope.Rising ? FontAwesome.Solid.ArrowUp : FontAwesome.Solid.ArrowDown;
        triggerBadge.Value = $"{scope.TriggerChannel.Name}  {SiFormat.Format(trigger.Level.Value, "V")}";
        triggerBadge.ValueColour = ScopeColours.ForChannel(trigger.Source.Value);
        triggerBadge.CaptionColour = ScopeColours.TRIGGER;
    }

    private void updateBottomBar()
    {
        foreach (var channel in scope.Channels)
        {
            var badge = channelBadges[channel.Index];
            badge.Value = $"{SiFormat.Format(channel.VoltsPerDivision.Value, "V")}  {channel.Coupling.Value}";
            badge.Alpha = channel.Enabled.Value ? 1 : 0.35f;
        }

        var math = scope.MathFunction.Value;
        double v1 = scope.Channels[0].VoltsPerDivision.Value, v2 = scope.Channels[1].VoltsPerDivision.Value;

        mathBadge.Value = math switch
        {
            MathMode.Add => $"1+2  {SiFormat.Format(v1, "V")}",
            MathMode.Subtract => $"1-2  {SiFormat.Format(v1, "V")}",
            MathMode.Multiply => $"1×2  {SiFormat.Format(v1 * v2, "V²")}",
            _ => mathBadge.Value,
        };

        fadeBadge(mathBadge, math != MathMode.Off && scope.DisplayMode.Value == DisplayMode.YT);
        fadeBadge(xyBadge, scope.DisplayMode.Value == DisplayMode.XY);

        double persistence = scope.Persistence.Value;
        persistenceBadge.Value = double.IsPositiveInfinity(persistence) ? "∞" : SiFormat.Format(persistence, "s", 2);
        fadeBadge(persistenceBadge, persistence > 0);
        xyBadge.ValueColour = ScopeColours.XY;
    }

    private void fadeBadge(Drawable badge, bool visible) =>
        badge.Alpha = (float)Interpolation.DampContinuously(badge.Alpha, visible ? 1 : 0, 40, Time.Elapsed);

    /// <summary>
    /// The phosphor area. Scroll to change the timebase, drag to pan horizontally.
    /// </summary>
    private partial class ScreenArea : Container
    {
        [Resolved]
        private Oscilloscope scope { get; set; } = null!;

        private readonly CursorReadout cursor;

        private float scrollRemainder;

        public ScreenArea(CursorReadout cursor)
        {
            this.cursor = cursor;

            Masking = true;
            CornerRadius = 6;
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // MousePosition is relative to this drawable's parent, so convert into the readout's own space instead.
            cursor.Target = cursor.ToLocalSpace(e.ScreenSpaceMousePosition);
            return base.OnMouseMove(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            cursor.Target = null;
            base.OnHoverLost(e);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            scrollRemainder += e.ScrollDelta.Y;

            int steps = (int)MathF.Truncate(scrollRemainder);
            scrollRemainder -= steps;

            if (steps == 0)
                return true;

            var timebase = scope.Timebase.SecondsPerDivision;
            int index = Math.Clamp(ScaleSteps.NearestIndex(Timebase.SECONDS_PER_DIVISION, timebase.Value) - steps, 0, Timebase.SECONDS_PER_DIVISION.Length - 1);
            timebase.Value = Timebase.SECONDS_PER_DIVISION[index];
            return true;
        }

        protected override bool OnDragStart(DragStartEvent e) => scope.DisplayMode.Value == DisplayMode.YT && !scope.IsRolling;

        protected override void OnDrag(DragEvent e)
        {
            var position = scope.Timebase.Position;
            position.Value = Math.Round(position.Value + e.Delta.X / DIVISION, 3);
        }
    }
}
