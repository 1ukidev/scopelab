using osu.Framework.Bindables;

namespace ScopeLab.Simulation;

public class Timebase
{
    public static readonly double[] SECONDS_PER_DIVISION = ScaleSteps.OneTwoFive(10e-9, 1);

    /// <summary>
    /// Timebases at or above this value switch the scope into roll mode while auto-triggering.
    /// </summary>
    public const double ROLL_THRESHOLD = 100e-3;

    public readonly BindableDouble SecondsPerDivision = new BindableDouble(1e-3);

    /// <summary>
    /// Horizontal location of the trigger point, in divisions from the centre of the screen.
    /// </summary>
    public readonly BindableDouble Position = new BindableDouble { MinValue = -5, MaxValue = 5 };

    public double Window => SecondsPerDivision.Value * Oscilloscope.HORIZONTAL_DIVISIONS;

    public void ResetToDefaults()
    {
        SecondsPerDivision.SetDefault();
        Position.SetDefault();
    }
}
