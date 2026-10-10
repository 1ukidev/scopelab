namespace ScopeLab.Simulation;

public enum AcquisitionState
{
    Stopped,
    Waiting,
    Triggered,
    Auto,
    Roll,
}

/// <summary>
/// The most recent sweep captured by the oscilloscope.
/// </summary>
public class Acquisition
{
    public const int SAMPLE_COUNT = 1000;

    public readonly double[][] Samples = [new double[SAMPLE_COUNT], new double[SAMPLE_COUNT]];

    public double StartTime { get; private set; }

    public double SampleInterval { get; private set; }

    /// <summary>
    /// The instant the horizontal axis is anchored to: the trigger point, or the newest sample while rolling.
    /// </summary>
    public double AnchorTime { get; private set; }

    public bool Rolling { get; private set; }

    /// <summary>
    /// Number of samples captured so far. Slow sweeps fill up progressively, like a beam crossing the screen.
    /// </summary>
    public int Count { get; set; }

    public bool Complete => Count == SAMPLE_COUNT;

    public void Begin(double startTime, double sampleInterval, double anchorTime, bool rolling)
    {
        StartTime = startTime;
        SampleInterval = sampleInterval;
        AnchorTime = anchorTime;
        Rolling = rolling;
        Count = 0;
    }

    public double TimeAt(int index) => StartTime + index * SampleInterval;
}
