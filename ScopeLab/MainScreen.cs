using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK;
using osuTK.Input;
using ScopeLab.Configuration;
using ScopeLab.Graphics;
using ScopeLab.Graphics.Containers;
using ScopeLab.Graphics.Controls;
using ScopeLab.Graphics.Display;
using ScopeLab.Graphics.Overlays;
using ScopeLab.Graphics.Panels;
using ScopeLab.Simulation;

namespace ScopeLab;

/// <summary>
/// The lab bench: an oscilloscope, its control panel and the function generator feeding it.
/// </summary>
public partial class MainScreen : Screen
{
    private const float margin = 16;
    private const float spacing = 12;
    private const float instrument_width = 862;
    private const float scope_height = 542;

    private static readonly Vector2 bench_size = ScopeLabGameBase.TARGET_SIZE - new Vector2(margin * 2);

    [Cached]
    private readonly Oscilloscope scope = new Oscilloscope();

    private InstrumentPanel scopeBody = null!;
    private InstrumentPanel controls = null!;
    private InstrumentPanel generator = null!;
    private AboutOverlay about = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        var display = new ScopeDisplay();

        InternalChildren =
        [
            scope,
            new ScopeStateStore(),
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourInfo.GradientVertical(ScopeColours.BENCH_LIGHT, ScopeColours.BENCH),
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = ScopeLabGameBase.TARGET_SIZE,
                Padding = new MarginPadding(margin),
                Children =
                [
                    scopeBody = new InstrumentPanel
                    {
                        Size = new Vector2(instrument_width, scope_height),
                        Children =
                        [
                            display,
                            new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = 222,
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                Children =
                                [
                                    createBranding(),
                                    new MeasurementPanel { Y = 44 },
                                ],
                            },
                        ],
                    },
                    generator = new InstrumentPanel
                    {
                        Size = new Vector2(instrument_width, bench_size.Y - scope_height - spacing),
                        Y = scope_height + spacing,
                        Child = new GeneratorPanel { RelativeSizeAxes = Axes.Both },
                    },
                    controls = new InstrumentPanel
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Size = new Vector2(bench_size.X - instrument_width - spacing, bench_size.Y),
                        Child = new ScopeControlPanel { RelativeSizeAxes = Axes.Both },
                    },
                ],
            },
            about = new AboutOverlay(),
        ];
    }

    private Drawable createBranding() => new Container
    {
        RelativeSizeAxes = Axes.X,
        AutoSizeAxes = Axes.Y,
        Children =
        [
            new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(6, 0),
                Children =
                [
                    new ScopeLabLogo
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(30),
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = "ScopeLab",
                        Font = ScopeFont.Regular(24, true),
                        Colour = ScopeColours.TEXT,
                    },
                ],
            },
            new ScopeButton(default, FontAwesome.Solid.InfoCircle)
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Size = new Vector2(28),
                FontSize = 16,
                Action = () => about.Show(),
                TooltipText = "About ScopeLab (F1)",
            },
        ],
    };

    public override void OnEntering(ScreenTransitionEvent e)
    {
        base.OnEntering(e);

        double delay = 0;

        foreach (var panel in new[] { scopeBody, controls, generator })
        {
            panel.FadeOut().MoveToOffset(new Vector2(0, 24))
                 .Delay(delay)
                 .FadeIn(600, Easing.OutQuint)
                 .MoveToOffset(new Vector2(0, -24), 800, Easing.OutQuint);

            delay += 90;
        }
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Repeat || e.ControlPressed || e.AltPressed)
            return false;

        switch (e.Key)
        {
            case Key.Space:
                scope.Running.Toggle();
                return true;

            case Key.S:
                scope.Single();
                return true;

            case Key.A:
                scope.AutoSet();
                return true;

            case Key.D:
                scope.ResetToDefaults();
                return true;

            case Key.F1:
                about.Show();
                return true;

            case Key.X:
                scope.DisplayMode.Value = scope.DisplayMode.Value == DisplayMode.YT ? DisplayMode.XY : DisplayMode.YT;
                return true;

            case Key.M:
                scope.MathFunction.Value = (MathMode)(((int)scope.MathFunction.Value + 1) % 4);
                return true;

            case Key.Number1:
            case Key.Keypad1:
                scope.Channels[0].Enabled.Toggle();
                return true;

            case Key.Number2:
            case Key.Keypad2:
                scope.Channels[1].Enabled.Toggle();
                return true;
        }

        return base.OnKeyDown(e);
    }
}
