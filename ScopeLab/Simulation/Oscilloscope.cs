using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;

namespace ScopeLab.Simulation;

public enum DisplayMode
{
    YT,
    XY,
}

public enum MathMode
{
    Off,
    Add,
    Subtract,
    Multiply,
}

/// <summary>
/// The acquisition engine: owns all instrument settings and captures sweeps from the generator outputs in real time.
/// </summary>
public partial class Oscilloscope : Component
{
    public const int HORIZONTAL_DIVISIONS = 10;
    public const int VERTICAL_DIVISIONS = 8;
    public const int CHANNEL_COUNT = 2;

    public static readonly double[] PERSISTENCE_STEPS = [0, 0.1, 0.25, 0.5, 1, 2, 5, double.PositiveInfinity];

    /// <summary>
    /// Sweeps longer than this are drawn progressively rather than appearing all at once.
    /// </summary>
    private const double progressive_sweep_threshold = 0.1;

    private const int max_trigger_scan_steps = 4096;

    /// <summary>
    /// Sweeps captured per second. The display only ever shows the latest sweep of each frame, so updating faster would waste work.
    /// </summary>
    private const double max_acquisition_rate = 120;

    private const ulong sample_dither_seed = 0x5DEECE66D;

    public readonly SignalGenerator[] Generators = [new SignalGenerator(0), new SignalGenerator(1)];

    public readonly ScopeChannel[] Channels;

    public readonly Timebase Timebase = new Timebase();

    public readonly TriggerSettings Trigger = new TriggerSettings();

    public readonly BindableBool Running = new BindableBool(true);

    public readonly Bindable<DisplayMode> DisplayMode = new Bindable<DisplayMode>();

    public readonly Bindable<MathMode> MathFunction = new Bindable<MathMode>();

    /// <summary>
    /// How long the phosphor keeps glowing, in seconds.
    /// </summary>
    public readonly BindableDouble Persistence = new BindableDouble { MinValue = 0, MaxValue = double.PositiveInfinity };

    public readonly BindableDouble Intensity = new BindableDouble(0.75) { MinValue = 0.1, MaxValue = 1 };

    public readonly Acquisition Acquisition = new Acquisition();

    public IBindable<AcquisitionState> State => state;

    public IBindable<bool> SingleArmed => singleArmed;

    /// <summary>
    /// Latest statistics of each channel, refreshed whenever a sweep completes.
    /// </summary>
    public readonly WaveformStatistics[] Statistics = [WaveformStatistics.EMPTY, WaveformStatistics.EMPTY];

    /// <summary>
    /// Invoked whenever an acquisition finishes.
    /// </summary>
    public event Action? SweepCompleted;

    private readonly Bindable<AcquisitionState> state = new Bindable<AcquisitionState>(AcquisitionState.Waiting);
    private readonly BindableBool singleArmed = new BindableBool();

    private double now;
    private double earliestNextTrigger;
    private double lastTriggerTime;
    private double lastAcquisitionTime = double.NegativeInfinity;
    private bool sweeping;

    public Oscilloscope()
    {
        Channels = Generators.Select((g, i) => new ScopeChannel(i, g)).ToArray();

        applyFactoryDefaults();

        Timebase.SecondsPerDivision.BindValueChanged(_ => sweeping = false);
        Timebase.Position.BindValueChanged(_ => sweeping = false);
        Running.BindValueChanged(e =>
        {
            if (!e.NewValue)
                singleArmed.Value = false;
        });
    }

    /// <summary>
    /// Establishes the factory setup, a sine and a triangle wave stacked on the screen, as the default of every setting.
    /// </summary>
    private void applyFactoryDefaults()
    {
        var second = Generators[1];
        setFactoryDefault(second.Shape, WaveformShape.Triangle);
        setFactoryDefault(second.Frequency, 1500);
        setFactoryDefault(second.Amplitude, 3);

        foreach (var channel in Channels)
            setFactoryDefault(channel.Position, channel.Index == 0 ? 2 : -2);

        setFactoryDefault(Timebase.SecondsPerDivision, 200e-6);

        static void setFactoryDefault<T>(Bindable<T> bindable, T value)
        {
            bindable.Default = value;
            bindable.SetDefault();
        }
    }

    /// <summary>
    /// Restores every setting of the scope and the generator to the factory setup, and resumes acquisition.
    /// </summary>
    public void ResetToDefaults()
    {
        foreach (var generator in Generators)
            generator.ResetToDefaults();

        foreach (var channel in Channels)
            channel.ResetToDefaults();

        Timebase.ResetToDefaults();
        Trigger.ResetToDefaults();

        DisplayMode.SetDefault();
        MathFunction.SetDefault();
        Persistence.SetDefault();
        Intensity.SetDefault();

        Running.Value = true;
    }

    public bool IsRolling => Trigger.Mode.Value == TriggerMode.Auto && !singleArmed.Value && Timebase.SecondsPerDivision.Value >= Timebase.ROLL_THRESHOLD;

    public ScopeChannel TriggerChannel => Channels[Trigger.Source.Value];

    /// <summary>
    /// Arms a single acquisition: the scope waits for the next trigger, captures one sweep and stops.
    /// </summary>
    public void Single()
    {
        sweeping = false;
        earliestNextTrigger = now;
        lastTriggerTime = double.NegativeInfinity;
        singleArmed.Value = true;
        Running.Value = true;
    }

    /// <summary>
    /// Places the trigger level halfway between the peaks of the trigger source.
    /// </summary>
    public void CentreTriggerLevel()
    {
        var statistics = Statistics[Trigger.Source.Value];

        if (!double.IsNaN(statistics.Max))
            Trigger.Level.Value = (statistics.Max + statistics.Min) / 2;
    }

    /// <summary>
    /// Reads the frequency of the trigger source with a reciprocal counter, far more precisely than waveform statistics allow.
    /// </summary>
    /// <remarks>
    /// Like a hardware counter running off the trigger comparator, this works at full bandwidth regardless of the displayed sample rate.
    /// </remarks>
    public double MeasureTriggerFrequency()
    {
        var channel = TriggerChannel;
        var generator = channel.Source;

        if (!channel.Enabled.Value || generator.Shape.Value == WaveformShape.Noise)
            return double.NaN;

        (double min, double max) = probeRange(channel, 1 / generator.Frequency.Value);

        return FrequencyCounter.Measure(t => channel.ProbeClean(t, true), (max + min) / 2, now, 0.1, generator.Frequency.Value);
    }

    protected override void Update()
    {
        base.Update();

        now = Time.Current / 1000;

        foreach (var generator in Generators)
            generator.Advance(now);

        if (!Running.Value)
        {
            state.Value = AcquisitionState.Stopped;
            return;
        }

        if (now - lastAcquisitionTime < 1 / max_acquisition_rate)
            return;

        lastAcquisitionTime = now;

        if (IsRolling)
        {
            roll();
            return;
        }

        if (sweeping)
            continueSweep();

        if (!sweeping && Running.Value)
            startSweep();
    }

    private void roll()
    {
        double interval = Timebase.Window / (Acquisition.SAMPLE_COUNT - 1);

        // Snap to the sample grid so the waveform scrolls instead of shimmering.
        double end = Math.Floor(now / interval) * interval;

        sweeping = false;
        Acquisition.Begin(end - Timebase.Window, interval, end, true);
        fill(Acquisition.SAMPLE_COUNT);

        state.Value = AcquisitionState.Roll;
        completeSweep();
    }

    private void startSweep()
    {
        double window = Timebase.Window;
        double preTrigger = (HORIZONTAL_DIVISIONS / 2.0 + Timebase.Position.Value) * Timebase.SecondsPerDivision.Value;
        double postTrigger = window - preTrigger;
        double interval = window / (Acquisition.SAMPLE_COUNT - 1);

        double searchEnd = window > progressive_sweep_threshold ? now : now - postTrigger;

        // How long the trigger may stay silent before the scope stops considering itself triggered.
        double triggerTimeout = Math.Max(0.05, window * 1.5);
        bool triggerLost = now - lastTriggerTime > triggerTimeout;

        if (findTrigger(earliestNextTrigger, searchEnd, interval, out double triggerTime))
        {
            lastTriggerTime = now;
            state.Value = AcquisitionState.Triggered;
        }
        else if (Trigger.Mode.Value == TriggerMode.Auto && !singleArmed.Value && triggerLost)
        {
            triggerTime = searchEnd;
            state.Value = AcquisitionState.Auto;
        }
        else
        {
            // Re-arming between periodic trigger events is momentary; only report waiting once the events actually stop.
            if (singleArmed.Value || triggerLost)
                state.Value = AcquisitionState.Waiting;

            return;
        }

        sweeping = true;
        Acquisition.Begin(triggerTime - preTrigger, interval, triggerTime, false);
        continueSweep();
    }

    private void continueSweep()
    {
        int available = (int)Math.Floor((now - Acquisition.StartTime) / Acquisition.SampleInterval) + 1;

        fill(Math.Clamp(available, 0, Acquisition.SAMPLE_COUNT));

        if (!Acquisition.Complete)
            return;

        sweeping = false;
        earliestNextTrigger = Acquisition.TimeAt(Acquisition.SAMPLE_COUNT - 1);
        completeSweep();

        if (singleArmed.Value)
            Running.Value = false;
    }

    private void fill(int count)
    {
        double interval = Acquisition.SampleInterval;

        for (int i = Acquisition.Count; i < count; i++)
        {
            double time = Acquisition.TimeAt(i);

            // Randomly dithered sample instants turn undersampled signals into a filled envelope instead of misleading aliases.
            // The dither is keyed on the absolute sample slot, so a rolling display scrolls without shimmering.
            time += (Noise.Uniform((long)Math.Round(time / interval), sample_dither_seed) - 0.5) * interval;

            for (int c = 0; c < CHANNEL_COUNT; c++)
                Acquisition.Samples[c][i] = Channels[c].Probe(time);
        }

        Acquisition.Count = Math.Max(Acquisition.Count, count);
    }

    private void completeSweep()
    {
        for (int c = 0; c < CHANNEL_COUNT; c++)
        {
            Statistics[c] = WaveformStatistics.Compute(Acquisition.Samples[c], Acquisition.SampleInterval,
                Channels[c].VoltsPerDivision.Value * 0.25);
        }

        SweepCompleted?.Invoke();
    }

    /// <summary>
    /// Scans the trigger source for the most recent qualifying edge within a time range.
    /// </summary>
    private bool findTrigger(double from, double to, double displayInterval, out double time)
    {
        time = 0;

        var channel = TriggerChannel;
        var generator = channel.Source;

        double step = displayInterval;
        if (generator.Shape.Value != WaveformShape.Noise)
            step = Math.Min(step, 1 / (generator.Frequency.Value * 32));

        from = Math.Max(from, to - step * max_trigger_scan_steps);

        if (to <= from)
            return false;

        double direction = Trigger.Slope.Value == TriggerSlope.Rising ? 1 : -1;
        double level = Trigger.Level.Value;
        double hysteresis = channel.VoltsPerDivision.Value * 0.25;

        double relative(double t) => direction * (channel.Probe(t) - level);

        int steps = (int)((to - from) / step);
        double previous = relative(from);
        bool armed = previous < -hysteresis;
        int crossing = -1;

        for (int i = 1; i <= steps; i++)
        {
            double current = relative(from + i * step);

            if (current < -hysteresis)
                armed = true;
            else if (armed && current >= 0 && previous < 0)
            {
                crossing = i;
                armed = false;
            }

            previous = current;
        }

        if (crossing < 0)
            return false;

        double low = from + (crossing - 1) * step;
        double high = from + crossing * step;

        for (int i = 0; i < 24; i++)
        {
            double middle = (low + high) / 2;

            if (relative(middle) >= 0)
                high = middle;
            else
                low = middle;
        }

        time = (low + high) / 2;
        return true;
    }

    /// <summary>
    /// Analyses the connected signals and picks sensible vertical, horizontal and trigger settings.
    /// </summary>
    public void AutoSet()
    {
        var active = Channels.Where(c => c.Source.Output.Value).ToArray();

        if (active.Length == 0)
            return;

        var reference = active.Contains(TriggerChannel) ? TriggerChannel : active[0];
        double period = reference.Source.Shape.Value == WaveformShape.Noise ? 1e-3 : 1 / reference.Source.Frequency.Value;

        foreach (var channel in Channels)
        {
            channel.Enabled.Value = active.Contains(channel);

            if (!channel.Enabled.Value)
                continue;

            if (channel.Coupling.Value == Coupling.GND)
                channel.Coupling.Value = Coupling.DC;

            (double min, double max) = probeRange(channel, period);

            // In YT mode two traces are stacked on separate halves of the screen, while XY mode needs both centred.
            bool stacked = active.Length > 1 && DisplayMode.Value == Simulation.DisplayMode.YT;
            double usableDivisions = stacked ? 3.5 : 6;
            double target = stacked ? (channel.Index == 0 ? 2 : -2) : 0;

            double voltsPerDivision = ScopeChannel.VOLTS_PER_DIVISION.FirstOrDefault(v => v * usableDivisions >= max - min, ScopeChannel.VOLTS_PER_DIVISION[^1]);

            channel.VoltsPerDivision.Value = voltsPerDivision;
            channel.Position.Value = Math.Round(target - (max + min) / 2 / voltsPerDivision, 2);

            if (channel == reference)
                Trigger.Level.Value = (max + min) / 2;
        }

        Timebase.SecondsPerDivision.Value = Timebase.SECONDS_PER_DIVISION.FirstOrDefault(s => s * HORIZONTAL_DIVISIONS >= period * 2.5, Timebase.SECONDS_PER_DIVISION[^1]);
        Timebase.Position.Value = 0;

        Trigger.Source.Value = reference.Index;
        Trigger.Slope.Value = TriggerSlope.Rising;
        Trigger.Mode.Value = TriggerMode.Auto;

        Running.Value = true;
    }

    private (double min, double max) probeRange(ScopeChannel channel, double period)
    {
        const int samples = 4000;

        double min = double.MaxValue, max = double.MinValue;

        for (int i = 0; i < samples; i++)
        {
            double v = channel.ProbeClean(now - 3 * period * i / samples);
            min = Math.Min(min, v);
            max = Math.Max(max, v);
        }

        return (min, max);
    }
}
