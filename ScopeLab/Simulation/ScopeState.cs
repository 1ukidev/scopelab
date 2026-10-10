using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using osu.Framework.Bindables;

namespace ScopeLab.Simulation;

/// <summary>
/// A serialisable snapshot of every user-adjustable setting of the scope and its generator.
/// </summary>
/// <remarks>
/// The run state is deliberately excluded, so the instrument always comes back running.
/// Every member is optional: anything missing or invalid is skipped when applying, so a damaged file can never break the instrument.
/// </remarks>
public record ScopeState
{
    private static readonly JsonSerializerOptions serializer_options = new JsonSerializerOptions
    {
        WriteIndented = true,
        NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
        Converters = { new JsonStringEnumConverter() },
    };

    public GeneratorState?[]? Generators { get; init; }

    public ChannelState?[]? Channels { get; init; }

    public double? SecondsPerDivision { get; init; }

    public double? HorizontalPosition { get; init; }

    public int? TriggerSource { get; init; }

    public TriggerSlope? TriggerSlope { get; init; }

    public TriggerMode? TriggerMode { get; init; }

    public double? TriggerLevel { get; init; }

    public DisplayMode? DisplayMode { get; init; }

    public MathMode? MathFunction { get; init; }

    public double? Persistence { get; init; }

    public double? Intensity { get; init; }

    public static ScopeState Capture(Oscilloscope scope) => new ScopeState
    {
        Generators = scope.Generators.Select(GeneratorState.Capture).ToArray(),
        Channels = scope.Channels.Select(ChannelState.Capture).ToArray(),
        SecondsPerDivision = scope.Timebase.SecondsPerDivision.Value,
        HorizontalPosition = scope.Timebase.Position.Value,
        TriggerSource = scope.Trigger.Source.Value,
        TriggerSlope = scope.Trigger.Slope.Value,
        TriggerMode = scope.Trigger.Mode.Value,
        TriggerLevel = scope.Trigger.Level.Value,
        DisplayMode = scope.DisplayMode.Value,
        MathFunction = scope.MathFunction.Value,
        Persistence = scope.Persistence.Value,
        Intensity = scope.Intensity.Value,
    };

    public void ApplyTo(Oscilloscope scope)
    {
        if (Generators != null)
        {
            for (int i = 0; i < Math.Min(Generators.Length, scope.Generators.Length); i++)
                Generators[i]?.ApplyTo(scope.Generators[i]);
        }

        if (Channels != null)
        {
            for (int i = 0; i < Math.Min(Channels.Length, scope.Channels.Length); i++)
                Channels[i]?.ApplyTo(scope.Channels[i]);
        }

        ApplyStep(scope.Timebase.SecondsPerDivision, SecondsPerDivision, Timebase.SECONDS_PER_DIVISION);
        ApplyFinite(scope.Timebase.Position, HorizontalPosition);

        if (TriggerSource is int source && source >= 0 && source < scope.Channels.Length)
            scope.Trigger.Source.Value = source;

        ApplyDefined(scope.Trigger.Slope, TriggerSlope);
        ApplyDefined(scope.Trigger.Mode, TriggerMode);
        ApplyFinite(scope.Trigger.Level, TriggerLevel);

        ApplyDefined(scope.DisplayMode, DisplayMode);
        ApplyDefined(scope.MathFunction, MathFunction);
        ApplyStep(scope.Persistence, Persistence, Oscilloscope.PERSISTENCE_STEPS);
        ApplyFinite(scope.Intensity, Intensity);
    }

    public string ToJson() => JsonSerializer.Serialize(this, serializer_options);

    public static bool TryParse(string json, out ScopeState? state)
    {
        try
        {
            state = JsonSerializer.Deserialize<ScopeState>(json, serializer_options);
            return state != null;
        }
        catch (JsonException)
        {
            state = null;
            return false;
        }
    }

    internal static void Apply(Bindable<bool> bindable, bool? value)
    {
        if (value is bool v)
            bindable.Value = v;
    }

    internal static void ApplyFinite(BindableDouble bindable, double? value)
    {
        if (value is double v && double.IsFinite(v))
            bindable.Value = v;
    }

    /// <summary>
    /// Snaps a value onto the positions of a stepped control.
    /// </summary>
    internal static void ApplyStep(BindableDouble bindable, double? value, double[] steps)
    {
        if (value is not double v)
            return;

        if (Array.IndexOf(steps, v) >= 0 || (double.IsFinite(v) && v > 0))
            bindable.Value = steps[ScaleSteps.NearestIndex(steps, v)];
    }

    internal static void ApplyDefined<T>(Bindable<T> bindable, T? value)
        where T : struct, Enum
    {
        if (value is T v && Enum.IsDefined(v))
            bindable.Value = v;
    }
}

public record GeneratorState
{
    public WaveformShape? Shape { get; init; }

    public double? Frequency { get; init; }

    public double? Amplitude { get; init; }

    public double? Offset { get; init; }

    public double? Phase { get; init; }

    public double? Noise { get; init; }

    public bool? Output { get; init; }

    public static GeneratorState Capture(SignalGenerator generator) => new GeneratorState
    {
        Shape = generator.Shape.Value,
        Frequency = generator.Frequency.Value,
        Amplitude = generator.Amplitude.Value,
        Offset = generator.Offset.Value,
        Phase = generator.Phase.Value,
        Noise = generator.NoiseLevel.Value,
        Output = generator.Output.Value,
    };

    public void ApplyTo(SignalGenerator generator)
    {
        ScopeState.ApplyDefined(generator.Shape, Shape);
        ScopeState.ApplyFinite(generator.Frequency, Frequency);
        ScopeState.ApplyFinite(generator.Amplitude, Amplitude);
        ScopeState.ApplyFinite(generator.Offset, Offset);
        ScopeState.ApplyFinite(generator.Phase, Phase);
        ScopeState.ApplyFinite(generator.NoiseLevel, Noise);
        ScopeState.Apply(generator.Output, Output);
    }
}

public record ChannelState
{
    public bool? Enabled { get; init; }

    public double? VoltsPerDivision { get; init; }

    public double? Position { get; init; }

    public Coupling? Coupling { get; init; }

    public static ChannelState Capture(ScopeChannel channel) => new ChannelState
    {
        Enabled = channel.Enabled.Value,
        VoltsPerDivision = channel.VoltsPerDivision.Value,
        Position = channel.Position.Value,
        Coupling = channel.Coupling.Value,
    };

    public void ApplyTo(ScopeChannel channel)
    {
        ScopeState.Apply(channel.Enabled, Enabled);
        ScopeState.ApplyStep(channel.VoltsPerDivision, VoltsPerDivision, ScopeChannel.VOLTS_PER_DIVISION);
        ScopeState.ApplyFinite(channel.Position, Position);
        ScopeState.ApplyDefined(channel.Coupling, Coupling);
    }
}
