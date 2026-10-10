using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// A dashed reference line spanning the screen, shown while adjusting levels and positions.
/// </summary>
public partial class GuideLine : CompositeDrawable
{
    private const int dashes = 64;

    public GuideLine(Direction direction, Colour4 colour)
    {
        bool horizontal = direction == Direction.Horizontal;

        RelativeSizeAxes = horizontal ? Axes.X : Axes.Y;
        Origin = horizontal ? Anchor.CentreLeft : Anchor.TopCentre;
        Colour = colour;
        Alpha = 0;

        if (horizontal)
            Height = 1;
        else
            Width = 1;

        var relativeAxis = horizontal ? Axes.X : Axes.Y;

        for (int i = 0; i < dashes; i++)
        {
            var dash = new Box
            {
                RelativeSizeAxes = relativeAxis,
                RelativePositionAxes = relativeAxis,
            };

            if (horizontal)
            {
                dash.Width = 0.55f / dashes;
                dash.Height = 1;
                dash.X = (float)i / dashes;
            }
            else
            {
                dash.Height = 0.55f / dashes;
                dash.Width = 1;
                dash.Y = (float)i / dashes;
            }

            AddInternal(dash);
        }
    }
}
