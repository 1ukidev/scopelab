using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using ScopeLab.Graphics.Containers;
using ScopeLab.Graphics.Controls;
using ScopeLab.Simulation;
using ScopeLab.Utils;

namespace ScopeLab.Graphics.Panels;

/// <summary>
/// The oscilloscope's front-panel controls.
/// </summary>
public partial class ScopeControlPanel : CompositeDrawable
{
    private const float column_spacing = 14;

    [Resolved]
    private Oscilloscope scope { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChild = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 14),
            Children =
            [
                createRunControls(),
                createHorizontalSection(),
                createVerticalSection(),
                createTriggerSection(),
                createDisplaySection(),
            ],
        };
    }

    private Drawable createRunControls() => new GridContainer
    {
        RelativeSizeAxes = Axes.X,
        Height = 40,
        ColumnDimensions =
        [
            new Dimension(), new Dimension(GridSizeMode.Absolute, 8),
            new Dimension(), new Dimension(GridSizeMode.Absolute, 8),
            new Dimension(), new Dimension(GridSizeMode.Absolute, 8),
            new Dimension(),
        ],
        Content = new[]
        {
            new Drawable[]
            {
                new ScopeButton("RUN / STOP")
                {
                    RelativeSizeAxes = Axes.Both,
                    Accent = ScopeColours.RUN,
                    Lit = scope.Running,
                    Action = () => scope.Running.Toggle(),
                    TooltipText = "Start or freeze the acquisition (Space)",
                    FontSize = 14,
                },
                Empty(),
                new ScopeButton("SINGLE")
                {
                    RelativeSizeAxes = Axes.Both,
                    Accent = ScopeColours.TRIGGER,
                    Lit = scope.SingleArmed,
                    Action = scope.Single,
                    TooltipText = "Capture one sweep on the next trigger, then stop (S)",
                    FontSize = 14,
                },
                Empty(),
                new ScopeButton("AUTO SET", FontAwesome.Solid.Magic)
                {
                    RelativeSizeAxes = Axes.Both,
                    Accent = ScopeColours.GENERATOR,
                    Action = scope.AutoSet,
                    TooltipText = "Analyse the inputs and pick sensible settings (A)",
                    FontSize = 14,
                },
                Empty(),
                new ScopeButton("DEFAULT", FontAwesome.Solid.Undo)
                {
                    RelativeSizeAxes = Axes.Both,
                    Action = scope.ResetToDefaults,
                    TooltipText = "Restore the factory setup of the scope and the generator (D)",
                    FontSize = 14,
                },
            },
        },
    };

    private Drawable createHorizontalSection() => new PanelSection("Horizontal")
    {
        Child = new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(column_spacing, 0),
            Children =
            [
                new Knob("Time/div", new SteppedScale(Timebase.SECONDS_PER_DIVISION), 54)
                {
                    Current = scope.Timebase.SecondsPerDivision,
                    Format = v => SiFormat.Format(v, "s"),
                    TooltipText = "Horizontal scale: time per grid division. You can also scroll over the screen.",
                },
                new Knob("Position", new EncoderScale(() => 0.05), 40)
                {
                    Current = scope.Timebase.Position,
                    Format = v => SiFormat.Format(v * scope.Timebase.SecondsPerDivision.Value, "s", signed: true),
                    TooltipText = "Moves the trigger point horizontally. You can also drag the screen. Double-click to centre.",
                },
                new TextFlowContainer(t =>
                {
                    t.Font = ScopeFont.Regular(12);
                    t.Colour = ScopeColours.TEXT_FAINT;
                })
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Width = 200,
                    AutoSizeAxes = Axes.Y,
                    Text = "Scroll over the screen to zoom in time, drag it to pan. Slow timebases switch to roll mode.",
                },
            ],
        },
    };

    private Drawable createVerticalSection() => new PanelSection("Vertical")
    {
        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = [new Dimension(), new Dimension(GridSizeMode.Absolute, 20), new Dimension()],
            RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
            Content = new[] { new[] { createChannelColumn(scope.Channels[0]), Empty(), createChannelColumn(scope.Channels[1]) } },
        },
    };

    private Drawable createChannelColumn(ScopeChannel channel)
    {
        var colour = ScopeColours.ForChannel(channel.Index);

        return new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 8),
            Children =
            [
                new ScopeButton(channel.Name)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 28,
                    Accent = colour,
                    Lit = channel.Enabled,
                    Action = () => channel.Enabled.Toggle(),
                    TooltipText = $"Show or hide {channel.Name} ({channel.Index + 1})",
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(column_spacing, 0),
                    Children =
                    [
                        new Knob("Volts/div", new SteppedScale(ScopeChannel.VOLTS_PER_DIVISION), 48)
                        {
                            Current = channel.VoltsPerDivision,
                            Accent = colour,
                            Format = v => SiFormat.Format(v, "V"),
                            TooltipText = $"Vertical scale of {channel.Name}: volts per grid division.",
                        },
                        new Knob("Position", new EncoderScale(() => 0.05), 40)
                        {
                            Current = channel.Position,
                            Accent = colour,
                            Format = v => $"{v:+0.00;-0.00;0.00} div",
                            TooltipText = $"Moves {channel.Name} up or down. You can also drag its marker. Double-click to reset.",
                        },
                    ],
                },
                new OptionSelector<Coupling>(Enum.GetValues<Coupling>())
                {
                    RelativeSizeAxes = Axes.X,
                    Current = channel.Coupling,
                    Accent = colour,
                    TooltipText = "DC passes everything, AC blocks the DC offset, GND shows the zero reference.",
                },
            ],
        };
    }

    private Drawable createTriggerSection() => new PanelSection("Trigger", ScopeColours.TRIGGER)
    {
        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            ColumnDimensions = [new Dimension(GridSizeMode.AutoSize), new Dimension(GridSizeMode.Absolute, 18), new Dimension()],
            RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
            Content = new[]
            {
                new Drawable[]
                {
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 4),
                        Children =
                        [
                            new Knob("Level", new EncoderScale(() => scope.TriggerChannel.VoltsPerDivision.Value / 25), 44)
                            {
                                Current = scope.Trigger.Level,
                                Accent = ScopeColours.TRIGGER,
                                Format = v => SiFormat.Format(v, "V"),
                                TooltipText = "The voltage the signal must cross to start a sweep. You can also drag the T marker.",
                            },
                            new ScopeButton("50%")
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(56, 22),
                                FontSize = 12,
                                Accent = ScopeColours.TRIGGER,
                                Action = scope.CentreTriggerLevel,
                                TooltipText = "Sets the level halfway between the signal's peaks",
                            },
                        ],
                    },
                    Empty(),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 8),
                        Children =
                        [
                            new LabelledRow("Source", new OptionSelector<int>([0, 1])
                            {
                                Current = scope.Trigger.Source,
                                Accent = ScopeColours.TRIGGER,
                                LabelFor = i => scope.Channels[i].Name,
                                TooltipText = "Which channel the trigger watches",
                            }),
                            new LabelledRow("Slope", new SlopeSelector
                            {
                                Current = scope.Trigger.Slope,
                                Accent = ScopeColours.TRIGGER,
                                TooltipText = "Trigger on rising or falling edges",
                            }),
                            new LabelledRow("Mode", new OptionSelector<TriggerMode>(Enum.GetValues<TriggerMode>())
                            {
                                Current = scope.Trigger.Mode,
                                Accent = ScopeColours.TRIGGER,
                                LabelFor = m => m.ToString().ToUpperInvariant(),
                                TooltipText = "AUTO sweeps even without a trigger, NORMAL waits for one",
                            }),
                        ],
                    },
                },
            },
        },
    };

    private Drawable createDisplaySection() => new PanelSection("Display")
    {
        Child = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 8),
            Children =
            [
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 26,
                    ColumnDimensions = [new Dimension(GridSizeMode.Relative, 0.4f), new Dimension(GridSizeMode.Absolute, 18), new Dimension()],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new LabelledRow("Format", new OptionSelector<DisplayMode>(Enum.GetValues<DisplayMode>())
                            {
                                Current = scope.DisplayMode,
                                Accent = ScopeColours.XY,
                                TooltipText = "YT plots voltage over time, XY plots CH1 against CH2 (X)",
                            }),
                            Empty(),
                            new LabelledRow("Math", new OptionSelector<MathMode>(Enum.GetValues<MathMode>())
                            {
                                Current = scope.MathFunction,
                                Accent = ScopeColours.MATH,
                                LabelFor = m => m switch
                                {
                                    MathMode.Add => "1+2",
                                    MathMode.Subtract => "1-2",
                                    MathMode.Multiply => "1×2",
                                    _ => "OFF",
                                },
                                TooltipText = "Combines both channels into a third trace (M)",
                            }, 40),
                        },
                    },
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(column_spacing, 0),
                    Children =
                    [
                        new Knob("Persistence", new SteppedScale(Oscilloscope.PERSISTENCE_STEPS), 40)
                        {
                            Current = scope.Persistence,
                            Accent = ScopeColours.XY,
                            Format = v => v switch
                            {
                                0 => "MIN",
                                double.PositiveInfinity => "∞",
                                _ => SiFormat.Format(v, "s", 2),
                            },
                            TooltipText = "How long the phosphor keeps glowing after the beam passes",
                        },
                        new Knob("Intensity", new LinearScale(0.1, 1, 0.02), 40)
                        {
                            Current = scope.Intensity,
                            Accent = ScopeColours.XY,
                            Format = v => $"{v * 100:0}%",
                            TooltipText = "Beam brightness",
                        },
                        new TextFlowContainer(t =>
                        {
                            t.Font = ScopeFont.Regular(12);
                            t.Colour = ScopeColours.TEXT_FAINT;
                        })
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Width = 220,
                            AutoSizeAxes = Axes.Y,
                            Text = "Shortcuts: Space run/stop, S single, A auto set, X toggle XY, M math, D default, 1/2 channels, F1 about.",
                        },
                    ],
                },
            ],
        },
    };
}
