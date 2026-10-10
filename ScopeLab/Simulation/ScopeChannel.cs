using osu.Framework.Bindables;

namespace ScopeLab.Simulation;

public enum Coupling
{
    DC,
    AC,
    GND,
}

/// <summary>
/// An analog input of the oscilloscope, probing one <see cref="SignalGenerator"/> output.
/// </summary>
public class ScopeChannel
{
    public static readonly double[] VOLTS_PER_DIVISION = ScaleSteps.OneTwoFive(1e-3, 10);

    /// <summary>
    /// Front-end noise of the vertical amplifier, in divisions RMS.
    /// </summary>
    private const double amplifier_noise = 0.01;

    public readonly int Index;

    public readonly SignalGenerator Source;

    public readonly BindableBool Enabled = new BindableBool(true);

    public readonly BindableDouble VoltsPerDivision = new BindableDouble(1);

    /// <summary>
    /// Vertical position of the ground reference, in divisions from the centre of the screen.
    /// </summary>
    public readonly BindableDouble Position = new BindableDouble { MinValue = -5, MaxValue = 5 };

    public readonly Bindable<Coupling> Coupling = new Bindable<Coupling>();

    private readonly ulong seed;

    public ScopeChannel(int index, SignalGenerator source)
    {
        Index = index;
        Source = source;
        seed = (ulong)(index + 1) * 0x9E3779B97F4A7C15;
    }

    public string Name => $"CH{Index + 1}";

    public void ResetToDefaults()
    {
        Enabled.SetDefault();
        VoltsPerDivision.SetDefault();
        Position.SetDefault();
        Coupling.SetDefault();
    }

    public double Probe(double time) => ProbeClean(time) + amplifier_noise * VoltsPerDivision.Value * Noise.Gaussian(time, seed);

    /// <summary>
    /// The voltage seen by the channel, excluding the amplifier's own noise.
    /// </summary>
    /// <param name="time">The instant to probe.</param>
    /// <param name="filtered">Whether to also reject the source's noise, as a frequency counter's input filter would.</param>
    public double ProbeClean(double time, bool filtered = false) => Coupling.Value switch
    {
        Simulation.Coupling.DC => Source.Sample(time, !filtered),
        Simulation.Coupling.AC => Source.Sample(time, !filtered) - Source.DcLevel,
        _ => 0,
    };

    /// <summary>
    /// Converts a voltage to a vertical screen position, in divisions above the centre line.
    /// </summary>
    public double ToDivisions(double volts) => volts / VoltsPerDivision.Value + Position.Value;

    public double ToVolts(double divisions) => (divisions - Position.Value) * VoltsPerDivision.Value;
}
