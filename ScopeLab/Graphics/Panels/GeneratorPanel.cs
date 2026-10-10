using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using ScopeLab.Graphics.Controls;
using ScopeLab.Simulation;
using ScopeLab.Utils;

namespace ScopeLab.Graphics.Panels;

/// <summary>
/// The front panel of the dual-output function generator feeding the oscilloscope.
/// </summary>
public partial class GeneratorPanel : CompositeDrawable
{
    [Resolved]
    private Oscilloscope scope { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren =
        [
            new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(8, 0),
                Children =
                [
                    new SpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Text = "FG",
                        Font = ScopeFont.Condensed(18, true),
                        Colour = ScopeColours.GENERATOR,
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Text = "FUNCTION GENERATOR  ·  20 MHz  ·  DUAL OUTPUT",
                        Font = ScopeFont.Condensed(12, true),
                        Colour = ScopeColours.TEXT_FAINT,
                        Spacing = new Vector2(1, 0),
                        Margin = new MarginPadding { Bottom = 2 },
                    },
                ],
            },
            new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Top = 32 },
                ColumnDimensions = [new Dimension(), new Dimension(GridSizeMode.Absolute, 28), new Dimension()],
                Content = new[]
                {
                    new[] { createOutput(scope.Channels[0]), Empty(), createOutput(scope.Channels[1]) },
                },
            },
        ];
    }

    private Drawable createOutput(ScopeChannel channel)
    {
        var generator = channel.Source;
        var colour = ScopeColours.ForChannel(channel.Index);

        return new FillFlowContainer
        {
            RelativeSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 8),
            Children =
            [
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 26,
                    ColumnDimensions = [new Dimension(GridSizeMode.Absolute, 96), new Dimension(GridSizeMode.Absolute, 10), new Dimension()],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new ScopeButton($"OUT {channel.Index + 1}")
                            {
                                RelativeSizeAxes = Axes.Both,
                                Accent = colour,
                                Lit = generator.Output,
                                Action = () => generator.Output.Toggle(),
                                TooltipText = $"Enables output {channel.Index + 1}, wired to {channel.Name} of the scope",
                            },
                            Empty(),
                            new WaveformSelector
                            {
                                RelativeSizeAxes = Axes.X,
                                Current = generator.Shape,
                                Accent = colour,
                                TooltipText = "Waveform",
                            },
                        },
                    },
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(6, 0),
                    Children =
                    [
                        new Knob("Freq", new LogScale(SignalGenerator.MIN_FREQUENCY, SignalGenerator.MAX_FREQUENCY, 24, 4), 42)
                        {
                            Current = generator.Frequency,
                            Accent = colour,
                            Format = v => SiFormat.Format(v, "Hz", 4),
                            TooltipText = "Frequency. Hold Shift for fine tuning: try slightly detuned channels in XY mode!",
                        },
                        new Knob("Ampl", new LogScale(0.01, 20, 20), 38)
                        {
                            Current = generator.Amplitude,
                            Accent = colour,
                            Format = v => SiFormat.Format(v, "Vpp"),
                            TooltipText = "Peak-to-peak amplitude",
                        },
                        new Knob("Offset", new LinearScale(-10, 10, 0.05), 38)
                        {
                            Current = generator.Offset,
                            Accent = colour,
                            Format = v => SiFormat.Format(v, "V", signed: true),
                            TooltipText = "DC offset added to the waveform",
                        },
                        new Knob("Phase", new LinearScale(-180, 180, 1), 38)
                        {
                            Current = generator.Phase,
                            Accent = colour,
                            Format = v => $"{v:+0;-0;0}°",
                            TooltipText = "Phase shift. Shapes the figures drawn in XY mode.",
                        },
                        new Knob("Noise", new LinearScale(0, 1, 0.005), 38)
                        {
                            Current = generator.NoiseLevel,
                            Accent = colour,
                            Format = v => SiFormat.Format(v, "V"),
                            TooltipText = "Adds random noise (RMS)",
                        },
                    ],
                },
            ],
        };
    }

    private partial class WaveformSelector() : OptionSelector<WaveformShape>(Enum.GetValues<WaveformShape>())
    {
        protected override Drawable CreateContent(WaveformShape item) => GlyphPath.Waveform(item, new Vector2(22, 11));
    }
}
