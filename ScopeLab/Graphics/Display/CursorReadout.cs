using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Utils;
using osuTK;
using ScopeLab.Simulation;
using ScopeLab.Utils;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// A crosshair following the mouse over the screen, reading out the time and voltages under it.
/// </summary>
public partial class CursorReadout : CompositeDrawable
{
    private const float label_offset = 14;

    [Resolved]
    private Oscilloscope scope { get; set; } = null!;

    /// <summary>
    /// The cursor location in local space, or <c>null</c> while the mouse is elsewhere.
    /// </summary>
    public Vector2? Target { get; set; }

    private readonly Box horizontalLine;
    private readonly Box verticalLine;
    private readonly Container label;
    private readonly SpriteText axisText;
    private readonly SpriteText[] channelTexts = new SpriteText[Oscilloscope.CHANNEL_COUNT];

    public CursorReadout()
    {
        RelativeSizeAxes = Axes.Both;
        Alpha = 0;
        AlwaysPresent = true;

        var lines = new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Padding = new MarginPadding { Horizontal = 7, Vertical = 4 },
            Child = axisText = createText(ScopeColours.TEXT),
        };

        for (int i = 0; i < channelTexts.Length; i++)
            lines.Add(channelTexts[i] = createText(ScopeColours.ForChannel(i)));

        InternalChildren =
        [
            horizontalLine = new Box { RelativeSizeAxes = Axes.X, Height = 1, Origin = Anchor.CentreLeft, Alpha = 0.18f },
            verticalLine = new Box { RelativeSizeAxes = Axes.Y, Width = 1, Origin = Anchor.TopCentre, Alpha = 0.18f },
            label = new Container
            {
                AutoSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 4,
                Children =
                [
                    new Box { RelativeSizeAxes = Axes.Both, Colour = ScopeColours.PANEL_DARK, Alpha = 0.85f },
                    lines,
                ],
            },
        ];
    }

    private static SpriteText createText(Colour4 colour) => new SpriteText
    {
        Font = ScopeFont.Numeric(12),
        Colour = colour,
    };

    protected override void Update()
    {
        base.Update();

        Alpha = (float)Interpolation.DampContinuously(Alpha, Target.HasValue ? 1 : 0, 30, Time.Elapsed);

        if (Target is not Vector2 target)
            return;

        horizontalLine.Y = target.Y;
        verticalLine.X = target.X;

        bool flipX = target.X + label_offset + label.DrawWidth > DrawWidth;
        bool flipY = target.Y + label_offset + label.DrawHeight > DrawHeight;

        label.Position = new Vector2(
            flipX ? target.X - label_offset - label.DrawWidth : target.X + label_offset,
            flipY ? target.Y - label_offset - label.DrawHeight : target.Y + label_offset);

        float division = DrawWidth / Oscilloscope.HORIZONTAL_DIVISIONS;
        double horizontal = target.X / division;
        double vertical = (DrawHeight / 2 - target.Y) / division;

        if (scope.DisplayMode.Value == DisplayMode.XY)
        {
            axisText.Text = "XY";
            channelTexts[0].Text = $"X {SiFormat.Format(scope.Channels[0].ToVolts(horizontal - Oscilloscope.HORIZONTAL_DIVISIONS / 2.0), "V")}";
            channelTexts[1].Text = $"Y {SiFormat.Format(scope.Channels[1].ToVolts(vertical), "V")}";

            foreach (var text in channelTexts)
                text.Alpha = 1;

            return;
        }

        double anchor = scope.IsRolling ? Oscilloscope.HORIZONTAL_DIVISIONS : Oscilloscope.HORIZONTAL_DIVISIONS / 2.0 + scope.Timebase.Position.Value;
        axisText.Text = $"t {SiFormat.Format((horizontal - anchor) * scope.Timebase.SecondsPerDivision.Value, "s", signed: true)}";

        foreach (var channel in scope.Channels)
        {
            var text = channelTexts[channel.Index];
            text.Text = $"{channel.Index + 1} {SiFormat.Format(channel.ToVolts(vertical), "V", signed: true)}";
            text.Alpha = channel.Enabled.Value ? 1 : 0;
        }
    }
}
