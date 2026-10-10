using System;
using osu.Framework.Bindables;

namespace ScopeLab.Simulation;

/// <summary>
/// One output of the bench function generator.
/// </summary>
public class SignalGenerator
{
    public const double MIN_FREQUENCY = 0.1;
    public const double MAX_FREQUENCY = 20e6;

    public readonly Bindable<WaveformShape> Shape = new Bindable<WaveformShape>();

    public readonly BindableDouble Frequency = new BindableDouble(1000) { MinValue = MIN_FREQUENCY, MaxValue = MAX_FREQUENCY };

    /// <summary>
    /// Peak-to-peak amplitude, in volts.
    /// </summary>
    public readonly BindableDouble Amplitude = new BindableDouble(2) { MinValue = 0.01, MaxValue = 20 };

    public readonly BindableDouble Offset = new BindableDouble { MinValue = -10, MaxValue = 10 };

    /// <summary>
    /// Phase shift, in degrees.
    /// </summary>
    public readonly BindableDouble Phase = new BindableDouble { MinValue = -180, MaxValue = 180 };

    /// <summary>
    /// Additive white noise, in volts RMS.
    /// </summary>
    public readonly BindableDouble NoiseLevel = new BindableDouble { MinValue = 0, MaxValue = 2 };

    public readonly BindableBool Output = new BindableBool(true);

    private readonly ulong seed;

    private double currentTime;
    private double originTime;
    private double originCycles;

    public SignalGenerator(int index)
    {
        seed = (ulong)(index + 1) * 0x2545F4914F6CDD1D;

        // Keep the phase continuous while retuning, just like a real DDS generator.
        Frequency.BindValueChanged(e =>
        {
            originCycles += (currentTime - originTime) * e.OldValue;
            originTime = currentTime;
        });
    }

    /// <summary>
    /// Informs the generator of the current simulation time, which is used as the pivot for phase-continuous retuning.
    /// </summary>
    public void Advance(double time) => currentTime = time;

    public void ResetToDefaults()
    {
        Shape.SetDefault();
        Frequency.SetDefault();
        Amplitude.SetDefault();
        Offset.SetDefault();
        Phase.SetDefault();
        NoiseLevel.SetDefault();
        Output.SetDefault();
    }

    /// <summary>
    /// The average output voltage.
    /// </summary>
    public double DcLevel => Output.Value ? Offset.Value + Amplitude.Value / 2 * Waveforms.Mean(Shape.Value) : 0;

    /// <param name="time">The instant to sample.</param>
    /// <param name="includeNoise">Whether to add the configured random noise.</param>
    public double Sample(double time, bool includeNoise = true)
    {
        if (!Output.Value)
            return 0;

        double value = Offset.Value;

        if (Shape.Value == WaveformShape.Noise)
            value += Amplitude.Value / 6 * Math.Clamp(Noise.Gaussian(time, ~seed), -3, 3);
        else
        {
            double cycles = originCycles + (time - originTime) * Frequency.Value + Phase.Value / 360;
            value += Amplitude.Value / 2 * Waveforms.Evaluate(Shape.Value, cycles, Frequency.Value);
        }

        if (includeNoise && NoiseLevel.Value > 0)
            value += NoiseLevel.Value * Noise.Gaussian(time, seed);

        return value;
    }
}
