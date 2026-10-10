using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using ScopeLab.Simulation;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// The measurement grid etched onto the screen.
/// </summary>
public partial class Graticule : CompositeDrawable
{
    private const int minor_ticks_per_division = 5;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Colour = ScopeColours.GRATICULE;

        for (int i = 0; i <= Oscilloscope.HORIZONTAL_DIVISIONS; i++)
            AddInternal(verticalLine((float)i / Oscilloscope.HORIZONTAL_DIVISIONS, i is 0 or Oscilloscope.HORIZONTAL_DIVISIONS ? 0.3f : 0.1f));

        for (int i = 0; i <= Oscilloscope.VERTICAL_DIVISIONS; i++)
            AddInternal(horizontalLine((float)i / Oscilloscope.VERTICAL_DIVISIONS, i is 0 or Oscilloscope.VERTICAL_DIVISIONS ? 0.3f : 0.1f));

        addTicks(Oscilloscope.HORIZONTAL_DIVISIONS, Axes.X);
        addTicks(Oscilloscope.VERTICAL_DIVISIONS, Axes.Y);
    }

    private static Box verticalLine(float x, float alpha) => new Box
    {
        RelativeSizeAxes = Axes.Y,
        RelativePositionAxes = Axes.X,
        Origin = Anchor.TopCentre,
        X = x,
        Width = 1,
        Alpha = alpha,
    };

    private static Box horizontalLine(float y, float alpha) => new Box
    {
        RelativeSizeAxes = Axes.X,
        RelativePositionAxes = Axes.Y,
        Origin = Anchor.CentreLeft,
        Y = y,
        Height = 1,
        Alpha = alpha,
    };

    /// <summary>
    /// Adds the fine subdivision ticks along the centre line of an axis.
    /// </summary>
    private void addTicks(int divisions, Axes along)
    {
        int count = divisions * minor_ticks_per_division;

        for (int i = 1; i < count; i++)
        {
            if (i % minor_ticks_per_division == 0)
                continue;

            float position = (float)i / count;

            AddInternal(new Box
            {
                RelativePositionAxes = Axes.Both,
                Origin = Anchor.Centre,
                Position = along == Axes.X ? new Vector2(position, 0.5f) : new Vector2(0.5f, position),
                Size = along == Axes.X ? new Vector2(1, 7) : new Vector2(7, 1),
                Alpha = 0.35f,
            });
        }
    }
}
