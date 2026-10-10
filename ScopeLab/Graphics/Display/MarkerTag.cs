using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace ScopeLab.Graphics.Display;

public enum MarkerDirection
{
    Right,
    Left,
    Down,
}

/// <summary>
/// A small arrow-shaped label, used for the channel, trigger level and trigger position markers.
/// </summary>
public partial class MarkerTag : CompositeDrawable
{
    private const float tip_length = 7;

    public MarkerTag(string text, Colour4 colour, MarkerDirection direction)
    {
        Colour = colour;

        var label = new Container
        {
            Masking = true,
            CornerRadius = 3,
            Children =
            [
                new Box { RelativeSizeAxes = Axes.Both },
                new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = text,
                    Font = ScopeFont.Condensed(12, true),
                    Colour = ScopeColours.SCREEN,
                },
            ],
        };

        var tip = new Triangle
        {
            Origin = Anchor.Centre,
            EdgeSmoothness = new Vector2(1),
        };

        switch (direction)
        {
            case MarkerDirection.Right:
                Size = new Vector2(18 + tip_length, 16);
                label.Size = new Vector2(18, 16);
                tip.Size = new Vector2(12, tip_length);
                tip.Rotation = 90;
                tip.Anchor = Anchor.CentreLeft;
                tip.X = 18 + tip_length / 2 - 0.5f;
                break;

            case MarkerDirection.Left:
                Size = new Vector2(18 + tip_length, 16);
                label.Size = new Vector2(18, 16);
                label.X = tip_length;
                tip.Size = new Vector2(12, tip_length);
                tip.Rotation = -90;
                tip.Anchor = Anchor.CentreLeft;
                tip.X = tip_length / 2 + 0.5f;
                break;

            case MarkerDirection.Down:
                Size = new Vector2(18, 14 + tip_length);
                label.Size = new Vector2(18, 14);
                tip.Size = new Vector2(10, tip_length);
                tip.Rotation = 180;
                tip.Anchor = Anchor.TopCentre;
                tip.Y = 14 + tip_length / 2 - 0.5f;
                break;
        }

        InternalChildren = [label, tip];
    }
}
