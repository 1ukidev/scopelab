using System;

namespace ScopeLab.Simulation;

/// <summary>
/// A reciprocal frequency counter: times a large number of whole periods for a precise reading.
/// </summary>
public static class FrequencyCounter
{
    private const int scan_steps = 48;
    private const int refinement_steps = 48;

    /// <param name="signal">The signal to measure.</param>
    /// <param name="level">The threshold whose rising crossings are timed.</param>
    /// <param name="end">The end of the gate window.</param>
    /// <param name="gate">The duration of the gate window.</param>
    /// <param name="roughFrequency">An approximate frequency, used to locate the first crossings.</param>
    public static double Measure(Func<double, double> signal, double level, double end, double gate, double roughFrequency)
    {
        if (!double.IsFinite(roughFrequency) || roughFrequency <= 0)
            return double.NaN;

        double period = 1 / roughFrequency;
        double start = end - Math.Max(gate, period * 2);

        if (!findRisingCrossing(signal, level, start, start + period * 1.5, out double first))
            return double.NaN;

        long cycles = 0;

        // Each step doubles the number of periods spanned, using the previous result to predict the next crossing.
        for (long n = 1; first + n * period <= end; n *= 2)
        {
            double predicted = first + n * period;

            if (!findRisingCrossing(signal, level, predicted - period * 0.4, predicted + period * 0.4, out double crossing))
                break;

            period = (crossing - first) / n;
            cycles = n;
        }

        return cycles > 0 ? 1 / period : double.NaN;
    }

    private static bool findRisingCrossing(Func<double, double> signal, double level, double from, double to, out double time)
    {
        time = 0;

        double step = (to - from) / scan_steps;
        double previous = signal(from) - level;

        for (int i = 1; i <= scan_steps; i++)
        {
            double t = from + i * step;
            double current = signal(t) - level;

            if (previous < 0 && current >= 0)
            {
                double low = t - step, high = t;

                for (int k = 0; k < refinement_steps; k++)
                {
                    double middle = (low + high) / 2;

                    if (signal(middle) >= level)
                        high = middle;
                    else
                        low = middle;
                }

                time = (low + high) / 2;
                return true;
            }

            previous = current;
        }

        return false;
    }
}
