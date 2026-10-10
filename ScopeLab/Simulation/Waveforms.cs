using System;
using System.Linq;

namespace ScopeLab.Simulation;

/// <summary>
/// Normalised (-1..1) periodic waveform functions.
/// </summary>
public static class Waveforms
{
    public const double PULSE_DUTY = 0.2;

    /// <summary>
    /// Time constant of the generator's output stage. Edges get a few nanoseconds of rise time and a slight overshoot,
    /// which only becomes visible when zooming into an edge.
    /// </summary>
    private const double edge_time_constant = 2.5e-9;

    private static readonly double[] means = Enum.GetValues<WaveformShape>().Select(computeMean).ToArray();

    public static double Evaluate(WaveformShape shape, double cycles, double frequency)
    {
        double phase = cycles - Math.Floor(cycles);

        return shape switch
        {
            WaveformShape.Sine => Math.Sin(Math.Tau * phase),
            WaveformShape.Square => edge(phase, 0.5, frequency),
            WaveformShape.Pulse => edge(phase, PULSE_DUTY, frequency),
            WaveformShape.Triangle => phase < 0.5 ? 4 * phase - 1 : 3 - 4 * phase,
            WaveformShape.Ramp => 2 * phase - 1 + 2 * (1 - stepResponse(phase / frequency)),
            WaveformShape.Cardiac => cardiac(phase),
            _ => 0,
        };
    }

    /// <summary>
    /// The DC component of a waveform, in normalised units.
    /// </summary>
    public static double Mean(WaveformShape shape) => means[(int)shape];

    private static double edge(double phase, double duty, double frequency) =>
        phase < duty
            ? -1 + 2 * stepResponse(phase / frequency)
            : 1 - 2 * stepResponse((phase - duty) / frequency);

    private static double stepResponse(double time)
    {
        double x = time / edge_time_constant;
        return x > 40 ? 1 : 1 - Math.Exp(-x) * (Math.Cos(x) + Math.Sin(x));
    }

    private static double cardiac(double phase)
    {
        double v = gaussian(phase, 0.18, 0.025, 0.12)
                   + gaussian(phase, 0.345, 0.009, -0.12)
                   + gaussian(phase, 0.37, 0.011, 1)
                   + gaussian(phase, 0.395, 0.01, -0.22)
                   + gaussian(phase, 0.62, 0.04, 0.28);

        return v * 1.64 - 0.64;

        static double gaussian(double x, double centre, double width, double height)
        {
            double d = (x - centre) / width;
            return height * Math.Exp(-0.5 * d * d);
        }
    }

    private static double computeMean(WaveformShape shape)
    {
        const int steps = 4096;

        double sum = 0;
        for (int i = 0; i < steps; i++)
            sum += Evaluate(shape, (i + 0.5) / steps, 1);

        return sum / steps;
    }
}
