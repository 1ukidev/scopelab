using System;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Lines;
using osuTK;
using ScopeLab.Simulation;

namespace ScopeLab.Graphics.Controls;

/// <summary>
/// Small line-art glyphs drawn from data, used for waveform and slope icons.
/// </summary>
public static class GlyphPath
{
    private const float stroke = 1.2f;

    public static Drawable Waveform(WaveformShape shape, Vector2 size)
    {
        const int points = 64;
        const double cycles = 2;

        var random = new Random(7);

        var vertices = Enumerable.Range(0, points).Select(i =>
        {
            double phase = i / (double)(points - 1) * cycles;
            double value = shape == WaveformShape.Noise
                ? random.NextDouble() * 2 - 1
                : Waveforms.Evaluate(shape, phase, 1);

            return new Vector2(i / (float)(points - 1) * size.X, (float)(1 - value) / 2 * size.Y);
        });

        return create(vertices.ToArray());
    }

    public static Drawable Edge(TriggerSlope slope, Vector2 size)
    {
        float low = slope == TriggerSlope.Rising ? size.Y : 0;
        float high = size.Y - low;

        return create(
        [
            new Vector2(0, low),
            new Vector2(size.X * 0.35f, low),
            new Vector2(size.X * 0.65f, high),
            new Vector2(size.X, high),
        ]);
    }

    private static Drawable create(Vector2[] vertices) => new SmoothPath
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        PathRadius = stroke,
        Vertices = vertices,
    };
}
