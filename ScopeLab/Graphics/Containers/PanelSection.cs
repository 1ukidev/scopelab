using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;

namespace ScopeLab.Graphics.Containers;

/// <summary>
/// A labelled group of controls on an instrument's front plate.
/// </summary>
public partial class PanelSection : Container
{
    private const float header_height = 20;

    private readonly Container content;

    protected override Container<Drawable> Content => content;

    public PanelSection(LocalisableString title, Colour4? accent = null)
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        InternalChildren =
        [
            new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(8, 0),
                Children =
                [
                    new Box
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(3, 12),
                        Colour = accent ?? ScopeColours.TEXT_DIM,
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = title.ToUpper(),
                        Font = ScopeFont.Condensed(13, true),
                        Colour = ScopeColours.TEXT_DIM,
                        Spacing = new Vector2(1.5f, 0),
                    },
                ],
            },
            new Box
            {
                RelativeSizeAxes = Axes.X,
                Height = 1,
                Y = header_height - 3,
                Colour = ScopeColours.BORDER,
                Alpha = 0.6f,
            },
            content = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Y = header_height + 6,
            },
        ];
    }
}
