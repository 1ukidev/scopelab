using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Input;
using ScopeLab.Graphics.Controls;
using ScopeLab.Localisation;

namespace ScopeLab.Graphics.Overlays;

/// <summary>
/// A short introduction to the program. Closes on Escape, the close button, or a click outside the dialog.
/// </summary>
public partial class AboutOverlay : OverlayContainer
{
    private const float dialog_width = 540;
    private const string repository_url = "https://github.com/1ukidev/scopelab";

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
                            createParagraph(AboutStrings.Introduction),
                            createParagraph(AboutStrings.Controls),
                            createParagraph(AboutStrings.Phosphor),
                            createTips(),
                            createFooter(),
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
                        Text = AboutStrings.Subtitle.ToUpper(),
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
                TooltipText = AboutStrings.CloseTooltip,
            },
        ],
    };

    private static Drawable createFooter() => new Container
    {
        RelativeSizeAxes = Axes.X,
        AutoSizeAxes = Axes.Y,
        Children =
        [
            new ExternalLink(repository_url, "1ukidev/scopelab", FontAwesome.Brands.Github)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                TooltipText = AboutStrings.RepositoryTooltip,
            },
            new SpriteText
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                Text = AboutStrings.BuiltWith,
                Font = ScopeFont.Condensed(12, true),
                Colour = ScopeColours.TEXT_FAINT,
            },
        ],
    };

    private static Drawable createParagraph(LocalisableString text) => new TextFlowContainer(t =>
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
                Text = AboutStrings.Tips,
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
