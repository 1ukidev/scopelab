using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Input;
using ScopeLab.Graphics.Controls;

namespace ScopeLab.Graphics.Overlays;

/// <summary>
/// A short introduction to the program. Closes on Escape, the close button, or a click outside the dialog.
/// </summary>
public partial class AboutOverlay : OverlayContainer
{
    private const float dialog_width = 540;

    private Container dialog = null!;

    protected override bool BlockNonPositionalInput => true;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children =
        [
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Black,
                Alpha = 0.6f,
            },
            dialog = new Dialog
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = dialog_width,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 16,
                BorderThickness = 1.5f,
                BorderColour = ScopeColours.BORDER,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Colour4.Black.Opacity(0.6f),
                    Radius = 24,
                    Offset = new Vector2(0, 8),
                },
                Children =
                [
                    new Box { RelativeSizeAxes = Axes.Both, Colour = ScopeColours.PANEL },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(24),
                        Spacing = new Vector2(0, 14),
                        Children =
                        [
                            createHeader(),
                            createParagraph("ScopeLab is a bench oscilloscope simulator. A dual-channel digital oscilloscope is wired to a two-output "
                                            + "function generator, so you can create signals and examine them just like on a real lab bench."),
                            createParagraph("Pick waveforms, frequencies and amplitudes on the generator, then use the timebase, vertical and trigger "
                                            + "controls to get a stable picture. Explore XY figures, math traces, persistence, automatic measurements "
                                            + "and the frequency counter."),
                            createParagraph("The screen imitates a phosphor display: the beam glows brighter where it moves slowly, fades on fast "
                                            + "edges and leaves an afterglow behind."),
                            createTips(),
                            new SpriteText
                            {
                                Text = "Built with osu!framework",
                                Font = ScopeFont.Condensed(12, true),
                                Colour = ScopeColours.TEXT_FAINT,
                            },
                        ],
                    },
                ],
            },
        ];
    }

    private Drawable createHeader() => new Container
    {
        RelativeSizeAxes = Axes.X,
        AutoSizeAxes = Axes.Y,
        Children =
        [
            new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 2),
                Children =
                [
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(8, 0),
                        Children =
                        [
                            new ScopeLabLogo
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Size = new Vector2(40),
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = "ScopeLab",
                                Font = ScopeFont.Regular(30, true),
                                Colour = ScopeColours.TEXT,
                            },
                        ],
                    },
                    new SpriteText
                    {
                        Text = "OSCILLOSCOPE & FUNCTION GENERATOR SIMULATOR",
                        Font = ScopeFont.Condensed(12, true),
                        Colour = ScopeColours.TEXT_FAINT,
                        Spacing = new Vector2(1, 0),
                    },
                ],
            },
            new ScopeButton(default, FontAwesome.Solid.Times)
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Size = new Vector2(30),
                Action = Hide,
                TooltipText = "Close (Esc)",
            },
        ],
    };

    private static Drawable createParagraph(string text) => new TextFlowContainer(t =>
    {
        t.Font = ScopeFont.Regular(15);
        t.Colour = ScopeColours.TEXT_DIM;
    })
    {
        RelativeSizeAxes = Axes.X,
        AutoSizeAxes = Axes.Y,
        Text = text,
    };

    private static Drawable createTips() => new Container
    {
        RelativeSizeAxes = Axes.X,
        AutoSizeAxes = Axes.Y,
        Masking = true,
        CornerRadius = 8,
        Children =
        [
            new Box { RelativeSizeAxes = Axes.Both, Colour = ScopeColours.PANEL_DARK },
            new TextFlowContainer(t =>
            {
                t.Font = ScopeFont.Regular(13);
                t.Colour = ScopeColours.TEXT_DIM;
            })
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding(14),
                ParagraphSpacing = 0.4f,
                Text = "Hover any control for a hint.\n"
                       + "Drag or scroll knobs, hold Shift for fine tuning and double-click to reset.\n"
                       + "Drag the markers on the screen edges, scroll over the screen to zoom in time.\n"
                       + "AUTO SET finds a picture for you, DEFAULT restores the factory setup.",
            },
        ],
    };

    protected override void PopIn()
    {
        this.FadeIn(250, Easing.OutQuint);
        dialog.ScaleTo(0.92f).ScaleTo(1, 500, Easing.OutQuint);
        dialog.MoveToY(16).MoveToY(0, 500, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        this.FadeOut(200, Easing.OutQuint);
        dialog.ScaleTo(0.96f, 200, Easing.OutQuint);
    }

    protected override bool OnClick(ClickEvent e)
    {
        Hide();
        return true;
    }

    protected override bool OnKeyDown(KeyDownEvent e)
    {
        if (e.Key == Key.Escape)
        {
            Hide();
            return true;
        }

        return base.OnKeyDown(e);
    }

    /// <summary>
    /// Swallows clicks so that only clicks on the backdrop dismiss the overlay.
    /// </summary>
    private partial class Dialog : Container
    {
        protected override bool OnClick(ClickEvent e) => true;
    }
}
