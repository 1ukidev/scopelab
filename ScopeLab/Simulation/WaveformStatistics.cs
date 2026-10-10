using System;

namespace ScopeLab.Simulation;

public readonly record struct WaveformStatistics(double Min, double Max, double Mean, double Rms, double Frequency, double DutyCycle)
{
    public static readonly WaveformStatistics EMPTY = new WaveformStatistics(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);

    public double PeakToPeak => Max - Min;

    public double Period => 1 / Frequency;

    /// <param name="samples">The captured voltages.</param>
    /// <param name="interval">Time between consecutive samples.</param>
    /// <param name="minimumSwing">Peak-to-peak swing below which timing measurements are considered meaningless.</param>
    public static WaveformStatistics Compute(ReadOnlySpan<double> samples, double interval, double minimumSwing)
    {
        if (samples.IsEmpty)
            return EMPTY;

        double min = double.MaxValue, max = double.MinValue, sum = 0, sumSquares = 0;

        foreach (double v in samples)
        {
            min = Math.Min(min, v);
            max = Math.Max(max, v);
            sum += v;
            sumSquares += v * v;
        }

        double mean = sum / samples.Length;
        double rms = Math.Sqrt(sumSquares / samples.Length);

        if (max - min < minimumSwing)
            return new WaveformStatistics(min, max, mean, rms, double.NaN, double.NaN);

        (double frequency, double duty) = measureTiming(samples, interval, (max + min) / 2, (max - min) * 0.2);

        return new WaveformStatistics(min, max, mean, rms, frequency, duty);
    }

    private static (double frequency, double duty) measureTiming(ReadOnlySpan<double> samples, double interval, double middle, double hysteresis)
    {
        var rising = new Crossings();
        var falling = new Crossings();

        bool armedRising = false, armedFalling = false;

        for (int i = 1; i < samples.Length; i++)
        {
            double previous = samples[i - 1], current = samples[i];

            armedRising |= current < middle - hysteresis;
            armedFalling |= current > middle + hysteresis;

            double crossing = i - 1 + (middle - previous) / (current - previous);

            if (armedRising && previous < middle && current >= middle)
            {
                rising.Add(crossing);
                armedRising = false;
            }
            else if (armedFalling && previous >= middle && current < middle)
            {
                falling.Add(crossing);
                armedFalling = false;
            }
        }

        // Both edge polarities give an estimate of the period, which allows measuring as little as ~1.5 cycles.
        int intervals = rising.Intervals + falling.Intervals;

        if (intervals == 0)
            return (double.NaN, double.NaN);

        double period = (rising.Span + falling.Span) / intervals;

        double duty = double.NaN;

        if (rising.Count > 0 && falling.Count > 0)
        {
            double high = falling.First - rising.First;
            duty = (high < 0 ? high + period : high) / period;
        }

        return (1 / (period * interval), Math.Clamp(duty, 0, 1));
    }

    private struct Crossings
    {
        public int Count;
        public double First;
        public double Last;

        public readonly int Intervals => Math.Max(0, Count - 1);

        public readonly double Span => Count > 1 ? Last - First : 0;

        public void Add(double position)
        {
            if (Count++ == 0)
                First = position;

            Last = position;
        }
    }
}
