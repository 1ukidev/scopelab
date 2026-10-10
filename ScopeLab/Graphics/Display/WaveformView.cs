using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osuTK;
using ScopeLab.Simulation;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// Projects the latest acquisition onto the phosphor, morphing smoothly between YT and XY presentations.
/// </summary>
public partial class WaveformView : CompositeDrawable
{
    private const int math_trace = Oscilloscope.CHANNEL_COUNT;

    [Resolved]
    private Oscilloscope scope { get; set; } = null!;

    private readonly PhosphorLayer phosphor;
    private readonly BeamTrace[] traces = [new BeamTrace(), new BeamTrace(), new BeamTrace()];
    private readonly float[] visibility = new float[3];

    private float xyMorph;

    public WaveformView()
    {
        RelativeSizeAxes = Axes.Both;
        InternalChild = phosphor = new PhosphorLayer { RelativeSizeAxes = Axes.Both };
        phosphor.Traces.AddRange(traces);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        scope.Persistence.BindValueChanged(p =>
        {
            phosphor.Persistence = p.NewValue > 0 ? p.NewValue : 0.05;
            phosphor.ClearAfterglow();
        }, true);

        scope.DisplayMode.BindValueChanged(_ => clearInfiniteAfterglow());
        scope.Timebase.SecondsPerDivision.BindValueChanged(_ => clearInfiniteAfterglow());

        foreach (var channel in scope.Channels)
        {
            channel.VoltsPerDivision.BindValueChanged(_ => clearInfiniteAfterglow());
            visibility[channel.Index] = channel.Enabled.Value ? 1 : 0;
        }

        xyMorph = scope.DisplayMode.Value == DisplayMode.XY ? 1 : 0;
    }

    private void clearInfiniteAfterglow()
    {
        if (double.IsPositiveInfinity(scope.Persistence.Value))
            phosphor.ClearAfterglow();
    }

    protected override void Update()
    {
        base.Update();

        double elapsed = Time.Elapsed;

        xyMorph = damp(xyMorph, scope.DisplayMode.Value == DisplayMode.XY ? 1 : 0, 70, elapsed);

        for (int c = 0; c < Oscilloscope.CHANNEL_COUNT; c++)
            visibility[c] = damp(visibility[c], scope.Channels[c].Enabled.Value ? 1 : 0, 40, elapsed);

        visibility[math_trace] = damp(visibility[math_trace], scope.MathFunction.Value != MathMode.Off ? 1 : 0, 40, elapsed);

        float division = DrawWidth / Oscilloscope.HORIZONTAL_DIVISIONS;
        float centreY = DrawHeight / 2;

        var acquisition = scope.Acquisition;
        double secondsPerDivision = scope.Timebase.SecondsPerDivision.Value;
        double anchorDivision = acquisition.Rolling ? Oscilloscope.HORIZONTAL_DIVISIONS : Oscilloscope.HORIZONTAL_DIVISIONS / 2.0 + scope.Timebase.Position.Value;

        var ch1 = scope.Channels[0];
        var ch2 = scope.Channels[1];
        double[] s1 = acquisition.Samples[0];
        double[] s2 = acquisition.Samples[1];

        (Func<double, double, double> function, double voltsPerDivision) = mathDefinition();

        bool showBeamSpot = scope.Running.Value && !acquisition.Complete && !acquisition.Rolling;

        for (int t = 0; t < traces.Length; t++)
        {
            var trace = traces[t];
            trace.Points.Clear();

            float morph = t == math_trace ? 0 : xyMorph;
            float intensity = (float)scope.Intensity.Value * visibility[t] * (t == math_trace ? 1 - xyMorph : 1 - 0.4f * xyMorph);

            trace.Intensity = intensity;
            trace.ReferenceSpacing = DrawWidth / (Acquisition.SAMPLE_COUNT - 1);
            trace.ShowBeamSpot = showBeamSpot;
            trace.Colour = t == math_trace ? ScopeColours.MATH : Interpolation.ValueAt(morph, ScopeColours.ForChannel(t), ScopeColours.XY, 0, 1);

            if (intensity <= 0.002f)
                continue;

            for (int i = 0; i < acquisition.Count; i++)
            {
                double divisions = t switch
                {
                    0 => ch1.ToDivisions(s1[i]),
                    1 => ch2.ToDivisions(s2[i]),
                    _ => function(s1[i], s2[i]) / voltsPerDivision,
                };

                var yt = new Vector2(
                    (float)((anchorDivision + (acquisition.TimeAt(i) - acquisition.AnchorTime) / secondsPerDivision) * division),
                    (float)(centreY - divisions * division));

                if (morph <= 0)
                {
                    trace.Points.Add(yt);
                    continue;
                }

                var xy = new Vector2(
                    (float)((Oscilloscope.HORIZONTAL_DIVISIONS / 2.0 + ch1.ToDivisions(s1[i])) * division),
                    (float)(centreY - ch2.ToDivisions(s2[i]) * division));

                trace.Points.Add(Vector2.Lerp(yt, xy, morph));
            }
        }

        phosphor.Refresh();
    }

    private (Func<double, double, double> function, double voltsPerDivision) mathDefinition()
    {
        double v1 = scope.Channels[0].VoltsPerDivision.Value;
        double v2 = scope.Channels[1].VoltsPerDivision.Value;

        return scope.MathFunction.Value switch
        {
            MathMode.Subtract => ((a, b) => a - b, v1),
            MathMode.Multiply => ((a, b) => a * b, v1 * v2),
            _ => ((a, b) => a + b, v1),
        };
    }

    private static float damp(float current, float target, double halfTime, double elapsed) =>
        (float)Interpolation.DampContinuously(current, target, halfTime, elapsed);
}
