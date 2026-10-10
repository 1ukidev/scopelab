using osu.Framework.Bindables;

namespace ScopeLab.Simulation;

public enum TriggerSlope
{
    Rising,
    Falling,
}

public enum TriggerMode
{
    Auto,
    Normal,
}

public class TriggerSettings
{
    /// <summary>
    /// Index of the channel the trigger listens to.
    /// </summary>
    public readonly Bindable<int> Source = new Bindable<int>();

    public readonly Bindable<TriggerSlope> Slope = new Bindable<TriggerSlope>();

    public readonly Bindable<TriggerMode> Mode = new Bindable<TriggerMode>();

    /// <summary>
    /// Trigger threshold, in volts.
    /// </summary>
    public readonly BindableDouble Level = new BindableDouble { MinValue = -100, MaxValue = 100 };

    public void ResetToDefaults()
    {
        Source.SetDefault();
        Slope.SetDefault();
        Mode.SetDefault();
        Level.SetDefault();
    }
}
