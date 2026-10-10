using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace ScopeLab.Graphics.Containers;

/// <summary>
/// The chassis of a bench instrument: a rounded, softly lit front plate.
/// </summary>
public partial class InstrumentPanel : Container
{
    private readonly Container content;

    protected override Container<Drawable> Content => content;

    public InstrumentPanel()
    {
        Masking = true;
        CornerRadius = 16;
        BorderThickness = 1.5f;
        BorderColour = ScopeColours.BORDER;
        EdgeEffect = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Colour = Colour4.Black.Opacity(0.55f),
            Radius = 18,
            Offset = new Vector2(0, 6),
        };

        InternalChildren =
        [
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourInfo.GradientVertical(ScopeColours.PANEL.Lighten(0.12f), ScopeColours.PANEL.Darken(0.15f)),
            },
            new Box
            {
                RelativeSizeAxes = Axes.X,
                Height = 1,
                Colour = Colour4.White.Opacity(0.08f),
            },
            content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(14),
            },
        ];
    }
}
