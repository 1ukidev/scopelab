using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;
using ScopeLab.Graphics.Containers;
using ScopeLab.Localisation;
using ScopeLab.Simulation;
using ScopeLab.Utils;

namespace ScopeLab.Graphics.Panels;

/// <summary>
/// Automatic measurements of both channels, plus a high-resolution counter on the trigger source.
/// </summary>
public partial class MeasurementPanel : CompositeDrawable
{
    private const double refresh_interval = 250;

    [Resolved]
    private Oscilloscope scope { get; set; } = null!;

    private readonly ChannelCard[] cards = new ChannelCard[Oscilloscope.CHANNEL_COUNT];
    private SpriteText counterText = null!;
    private SpriteText counterSource = null!;

    public MeasurementPanel()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        var flow = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 10),
        };

        foreach (var channel in scope.Channels)
            flow.Add(cards[channel.Index] = new ChannelCard(channel));

        flow.Add(createCounter());

        InternalChild = new PanelSection(DisplayStrings.Measure) { Child = flow };
    }

    private Drawable createCounter() => new Container
    {
        RelativeSizeAxes = Axes.X,
        Height = 58,
        Masking = true,
        CornerRadius = 8,
        Children =
        [
            new Box { RelativeSizeAxes = Axes.Both, Colour = ScopeColours.PANEL_DARK },
            new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding { Horizontal = 12, Vertical = 8 },
                Spacing = new Vector2(0, 2),
                Children =
                [
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(6, 0),
                        Children =
                        [
                            new SpriteText
                            {
                                Text = DisplayStrings.Counter.ToUpper(),
                                Font = ScopeFont.Condensed(11, true),
                                Colour = ScopeColours.TEXT_FAINT,
                            },
                            counterSource = new SpriteText
                            {
                                Font = ScopeFont.Condensed(11, true),
                                Colour = ScopeColours.TRIGGER,
                            },
                        ],
                    },
                    counterText = new SpriteText
                    {
                        Font = ScopeFont.Numeric(22),
                        Colour = ScopeColours.TEXT,
                    },
                ],
            },
        ],
    };

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Scheduler.AddDelayed(refresh, refresh_interval, true);
        refresh();
    }

    private void refresh()
    {
        foreach (var card in cards)
            card.Refresh(scope.Statistics[card.Channel.Index]);

        var source = scope.TriggerChannel;
        double frequency = scope.MeasureTriggerFrequency();

        counterSource.Text = source.Name;
        counterSource.Colour = ScopeColours.ForChannel(source.Index);
        counterText.Text = source.Enabled.Value && double.IsFinite(frequency) ? SiFormat.Format(frequency, "Hz", 6) : "--------";
    }

    private partial class ChannelCard : CompositeDrawable
    {
        public readonly ScopeChannel Channel;

        private readonly Readout frequency = new Readout(DisplayStrings.Frequency);
        private readonly Readout period = new Readout(DisplayStrings.Period);
        private readonly Readout peakToPeak = new Readout("Vpp");
        private readonly Readout rms = new Readout("Vrms");
        private readonly Readout mean = new Readout(DisplayStrings.Mean);
        private readonly Readout duty = new Readout(DisplayStrings.DutyCycle);
        private readonly Readout maximum = new Readout(DisplayStrings.Maximum);
        private readonly Readout minimum = new Readout(DisplayStrings.Minimum);

        private SpriteText couplingText = null!;
        private WaveformStatistics smoothed = WaveformStatistics.EMPTY;

        public ChannelCard(ScopeChannel channel)
        {
            Channel = channel;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var colour = ScopeColours.ForChannel(Channel.Index);

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 8,
                Children =
                [
                    new Box { RelativeSizeAxes = Axes.Both, Colour = ScopeColours.PANEL_DARK },
                    new Box { RelativeSizeAxes = Axes.Y, Width = 3, Colour = colour },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding { Left = 14, Right = 10, Vertical = 8 },
                        Spacing = new Vector2(0, 6),
                        Children =
                        [
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children =
                                [
                                    new SpriteText
                                    {
                                        Text = Channel.Name,
                                        Font = ScopeFont.Condensed(14, true),
                                        Colour = colour,
                                    },
                                    couplingText = new SpriteText
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Font = ScopeFont.Condensed(11, true),
                                        Colour = ScopeColours.TEXT_FAINT,
                                    },
                                ],
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                RowDimensions = [new Dimension(GridSizeMode.AutoSize), new Dimension(GridSizeMode.AutoSize), new Dimension(GridSizeMode.AutoSize), new Dimension(GridSizeMode.AutoSize)],
                                Content = new[]
                                {
                                    new Drawable[] { frequency, period },
                                    new Drawable[] { peakToPeak, rms },
                                    new Drawable[] { mean, duty },
                                    new Drawable[] { maximum, minimum },
                                },
                            },
                        ],
                    },
                ],
            };
        }

        public void Refresh(WaveformStatistics latest)
        {
            smoothed = smoothed with
            {
                Min = smooth(smoothed.Min, latest.Min),
                Max = smooth(smoothed.Max, latest.Max),
                Mean = smooth(smoothed.Mean, latest.Mean),
                Rms = smooth(smoothed.Rms, latest.Rms),
                Frequency = smooth(smoothed.Frequency, latest.Frequency),
                DutyCycle = smooth(smoothed.DutyCycle, latest.DutyCycle),
            };

            bool enabled = Channel.Enabled.Value;
            this.FadeTo(enabled ? 1 : 0.4f, 300, Easing.OutQuint);
            couplingText.Text = $"{Channel.Coupling.Value} · {SiFormat.Format(Channel.VoltsPerDivision.Value, "V")}/div";

            var s = enabled ? smoothed : WaveformStatistics.EMPTY;

            frequency.Value = SiFormat.Format(s.Frequency, "Hz", 4);
            period.Value = SiFormat.Format(s.Period, "s", 4);
            peakToPeak.Value = SiFormat.Format(s.PeakToPeak, "V");
            rms.Value = SiFormat.Format(s.Rms, "V");
            mean.Value = SiFormat.Format(s.Mean, "V");
            duty.Value = double.IsNaN(s.DutyCycle) ? "--- %" : FormattableString.Invariant($"{s.DutyCycle * 100:0.0} %");
            maximum.Value = SiFormat.Format(s.Max, "V");
            minimum.Value = SiFormat.Format(s.Min, "V");
        }

        /// <summary>
        /// Averages successive readings so the digits settle, while still following large changes promptly.
        /// </summary>
        private static double smooth(double current, double latest)
        {
            if (!double.IsFinite(current) || !double.IsFinite(latest))
                return latest;

            double scale = Math.Max(Math.Abs(current), Math.Abs(latest));
            bool largeChange = scale > 0 && Math.Abs(latest - current) / scale > 0.1;

            return largeChange ? latest : current + (latest - current) * 0.4;
        }
    }

    private partial class Readout : CompositeDrawable
    {
        private readonly SpriteText valueText;

        public Readout(LocalisableString label)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children =
                [
                    new SpriteText
                    {
                        Text = label.ToUpper(),
                        Font = ScopeFont.Condensed(10, true),
                        Colour = ScopeColours.TEXT_FAINT,
                    },
                    valueText = new SpriteText
                    {
                        Font = ScopeFont.Numeric(15),
                        Colour = ScopeColours.TEXT,
                    },
                ],
            };
        }

        public string Value
        {
            set => valueText.Text = value;
        }
    }
}
