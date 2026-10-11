using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace ScopeLab.Graphics.Containers;

/// <summary>
/// A caption followed by a control that fills the remaining width.
/// </summary>
public partial class LabelledRow : CompositeDrawable
{
    public LabelledRow(LocalisableString label, Drawable control, float labelWidth = 52)
    {
        RelativeSizeAxes = Axes.X;
        Height = control.Height;

        control.RelativeSizeAxes = Axes.X;
        control.Width = 1;

        InternalChildren =
        [
            new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Text = label.ToUpper(),
                Font = ScopeFont.Condensed(11, true),
                Colour = ScopeColours.TEXT_FAINT,
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Left = labelWidth },
                Child = control,
            },
        ];
    }
}
