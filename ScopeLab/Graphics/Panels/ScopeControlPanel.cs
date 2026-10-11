using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using ScopeLab.Graphics.Containers;
using ScopeLab.Graphics.Controls;
using ScopeLab.Localisation;
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
        InternalChildren =
        [
            new FillFlowContainer
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
            },
            new FillFlowContainer
            {
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(8, 0),
                Children =
                [
                    new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Icon = FontAwesome.Solid.Globe,
                        Size = new Vector2(14),
                        Colour = ScopeColours.TEXT_FAINT,
                    },
                    new LanguageSelector
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Width = 84,
                        Height = 24,
                        Accent = ScopeColours.TEXT,
                    },
                ],
            },
        ];
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
                new ScopeButton(ControlStrings.RunStop)
                {
                    RelativeSizeAxes = Axes.Both,
                    Accent = ScopeColours.RUN,
                    Lit = scope.Running,
                    Action = () => scope.Running.Toggle(),
                    TooltipText = ControlStrings.RunStopTooltip,
                    FontSize = 14,
                },
                Empty(),
                new ScopeButton(ControlStrings.Single)
                {
                    RelativeSizeAxes = Axes.Both,
                    Accent = ScopeColours.TRIGGER,
                    Lit = scope.SingleArmed,
                    Action = scope.Single,
                    TooltipText = ControlStrings.SingleTooltip,
                    FontSize = 14,
                },
                Empty(),
                new ScopeButton(ControlStrings.AutoSet, FontAwesome.Solid.Magic)
                {
                    RelativeSizeAxes = Axes.Both,
                    Accent = ScopeColours.GENERATOR,
                    Action = scope.AutoSet,
                    TooltipText = ControlStrings.AutoSetTooltip,
                    FontSize = 14,
                },
                Empty(),
                new ScopeButton(ControlStrings.Default, FontAwesome.Solid.Undo)
                {
                    RelativeSizeAxes = Axes.Both,
                    Action = scope.ResetToDefaults,
                    TooltipText = ControlStrings.DefaultTooltip,
                    FontSize = 14,
                },
            },
        },
    };

    private Drawable createHorizontalSection() => new PanelSection(ControlStrings.Horizontal)
    {
        Child = new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(column_spacing, 0),
            Children =
            [
                new Knob(ControlStrings.TimePerDivision, new SteppedScale(Timebase.SECONDS_PER_DIVISION), 54)
                {
                    Current = scope.Timebase.SecondsPerDivision,
                    Format = v => SiFormat.Format(v, "s"),
                    TooltipText = ControlStrings.TimePerDivisionTooltip,
                },
                new Knob(ControlStrings.Position, new EncoderScale(() => 0.05), 40)
                {
                    Current = scope.Timebase.Position,
                    Format = v => SiFormat.Format(v * scope.Timebase.SecondsPerDivision.Value, "s", signed: true),
                    TooltipText = ControlStrings.HorizontalPositionTooltip,
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
                    Text = ControlStrings.HorizontalHint,
                },
            ],
        },
    };

    private Drawable createVerticalSection() => new PanelSection(ControlStrings.Vertical)
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
                    TooltipText = ControlStrings.ChannelTooltip(channel.Name, channel.Index + 1),
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
                        new Knob(ControlStrings.VoltsPerDivision, new SteppedScale(ScopeChannel.VOLTS_PER_DIVISION), 48)
                        {
                            Current = channel.VoltsPerDivision,
                            Accent = colour,
                            Format = v => SiFormat.Format(v, "V"),
                            TooltipText = ControlStrings.VoltsPerDivisionTooltip(channel.Name),
                        },
                        new Knob(ControlStrings.Position, new EncoderScale(() => 0.05), 40)
                        {
                            Current = channel.Position,
                            Accent = colour,
                            Format = v => FormattableString.Invariant($"{v:+0.00;-0.00;0.00} div"),
                            TooltipText = ControlStrings.ChannelPositionTooltip(channel.Name),
                        },
                    ],
                },
                new OptionSelector<Coupling>(Enum.GetValues<Coupling>())
                {
                    RelativeSizeAxes = Axes.X,
                    Current = channel.Coupling,
                    Accent = colour,
                    TooltipText = ControlStrings.CouplingTooltip,
                },
            ],
        };
    }

    private Drawable createTriggerSection() => new PanelSection(ControlStrings.Trigger, ScopeColours.TRIGGER)
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
                            new Knob(ControlStrings.Level, new EncoderScale(() => scope.TriggerChannel.VoltsPerDivision.Value / 25), 44)
                            {
                                Current = scope.Trigger.Level,
                                Accent = ScopeColours.TRIGGER,
                                Format = v => SiFormat.Format(v, "V"),
                                TooltipText = ControlStrings.LevelTooltip,
                            },
                            new ScopeButton("50%")
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(56, 22),
                                FontSize = 12,
                                Accent = ScopeColours.TRIGGER,
                                Action = scope.CentreTriggerLevel,
                                TooltipText = ControlStrings.CentreLevelTooltip,
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
                            new LabelledRow(ControlStrings.Source, new OptionSelector<int>([0, 1])
                            {
                                Current = scope.Trigger.Source,
                                Accent = ScopeColours.TRIGGER,
                                LabelFor = i => scope.Channels[i].Name,
                                TooltipText = ControlStrings.SourceTooltip,
                            }),
                            new LabelledRow(ControlStrings.Slope, new SlopeSelector
                            {
                                Current = scope.Trigger.Slope,
                                Accent = ScopeColours.TRIGGER,
                                TooltipText = ControlStrings.SlopeTooltip,
                            }),
                            new LabelledRow(ControlStrings.Mode, new OptionSelector<TriggerMode>(Enum.GetValues<TriggerMode>())
                            {
                                Current = scope.Trigger.Mode,
                                Accent = ScopeColours.TRIGGER,
                                LabelFor = m => m.ToString().ToUpperInvariant(),
                                TooltipText = ControlStrings.ModeTooltip,
                            }),
                        ],
                    },
                },
            },
        },
    };

    private Drawable createDisplaySection() => new PanelSection(ControlStrings.Display)
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
                            new LabelledRow(ControlStrings.Format, new OptionSelector<DisplayMode>(Enum.GetValues<DisplayMode>())
                            {
                                Current = scope.DisplayMode,
                                Accent = ScopeColours.XY,
                                TooltipText = ControlStrings.FormatTooltip,
                            }),
                            Empty(),
                            new LabelledRow(ControlStrings.Math, new OptionSelector<MathMode>(Enum.GetValues<MathMode>())
                            {
                                Current = scope.MathFunction,
                                Accent = ScopeColours.MATH,
                                LabelFor = m => m switch
                                {
                                    MathMode.Add => "1+2",
                                    MathMode.Subtract => "1-2",
                                    MathMode.Multiply => "1×2",
                                    _ => ControlStrings.MathOff,
                                },
                                TooltipText = ControlStrings.MathTooltip,
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
                        new Knob(ControlStrings.Persistence, new SteppedScale(Oscilloscope.PERSISTENCE_STEPS), 40)
                        {
                            Current = scope.Persistence,
                            Accent = ScopeColours.XY,
                            Format = v => v switch
                            {
                                0 => ControlStrings.PersistenceMinimum,
                                double.PositiveInfinity => "∞",
                                _ => SiFormat.Format(v, "s", 2),
                            },
                            TooltipText = ControlStrings.PersistenceTooltip,
                        },
                        new Knob(ControlStrings.Intensity, new LinearScale(0.1, 1, 0.02), 40)
                        {
                            Current = scope.Intensity,
                            Accent = ScopeColours.XY,
                            Format = v => FormattableString.Invariant($"{v * 100:0}%"),
                            TooltipText = ControlStrings.IntensityTooltip,
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
                            Text = ControlStrings.Shortcuts,
                        },
                    ],
                },
            ],
        },
    };
}
