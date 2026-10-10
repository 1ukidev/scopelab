using NUnit.Framework;
using ScopeLab.Simulation;

namespace ScopeLab.Tests.NonVisual;

[TestFixture]
public class ScopeStateTest
{
    [Test]
    public void TestRoundTrip()
    {
        var original = createCustomisedScope();
        string json = ScopeState.Capture(original).ToJson();

        var restored = new Oscilloscope();
        Assert.That(ScopeState.TryParse(json, out var state), Is.True);
        state!.ApplyTo(restored);

        Assert.That(ScopeState.Capture(restored).ToJson(), Is.EqualTo(json));
    }

    [Test]
    public void TestResetRestoresFactorySetup()
    {
        var scope = createCustomisedScope();
        scope.Running.Value = false;

        scope.ResetToDefaults();

        Assert.Multiple(() =>
        {
            Assert.That(ScopeState.Capture(scope).ToJson(), Is.EqualTo(ScopeState.Capture(new Oscilloscope()).ToJson()));
            Assert.That(scope.Running.Value, Is.True);
        });
    }

    [Test]
    public void TestCorruptJsonIsRejected() => Assert.That(ScopeState.TryParse("{ \"Generators\": [", out _), Is.False);

    [Test]
    public void TestMissingAndInvalidValuesAreIgnored()
    {
        var scope = new Oscilloscope();
        string factory = ScopeState.Capture(scope).ToJson();

        const string json = """
                            {
                              "Generators": [ null, { "Frequency": "NaN" } ],
                              "Channels": [ { "VoltsPerDivision": -1 } ],
                              "SecondsPerDivision": 0,
                              "TriggerSource": 7,
                              "DisplayMode": 9
                            }
                            """;

        Assert.That(ScopeState.TryParse(json, out var state), Is.True);
        state!.ApplyTo(scope);

        Assert.That(ScopeState.Capture(scope).ToJson(), Is.EqualTo(factory));
    }

    [Test]
    public void TestOffStepValuesSnapToNearestStep()
    {
        var scope = new Oscilloscope();

        new ScopeState { SecondsPerDivision = 0.0019, Persistence = 0.3 }.ApplyTo(scope);

        Assert.Multiple(() =>
        {
            Assert.That(scope.Timebase.SecondsPerDivision.Value, Is.EqualTo(2e-3));
            Assert.That(scope.Persistence.Value, Is.EqualTo(0.25));
        });
    }

    private static Oscilloscope createCustomisedScope()
    {
        var scope = new Oscilloscope();

        var generator = scope.Generators[0];
        generator.Shape.Value = WaveformShape.Pulse;
        generator.Frequency.Value = 1234.5;
        generator.Amplitude.Value = 4.2;
        generator.Offset.Value = -0.5;
        generator.Phase.Value = 45;
        generator.NoiseLevel.Value = 0.2;
        scope.Generators[1].Output.Value = false;

        var channel = scope.Channels[1];
        channel.Enabled.Value = false;
        channel.VoltsPerDivision.Value = 0.5;
        channel.Position.Value = 1.25;
        channel.Coupling.Value = Coupling.AC;

        scope.Timebase.SecondsPerDivision.Value = 5e-3;
        scope.Timebase.Position.Value = -1.5;
        scope.Trigger.Source.Value = 1;
        scope.Trigger.Slope.Value = TriggerSlope.Falling;
        scope.Trigger.Mode.Value = TriggerMode.Normal;
        scope.Trigger.Level.Value = 0.7;
        scope.DisplayMode.Value = DisplayMode.XY;
        scope.MathFunction.Value = MathMode.Multiply;
        scope.Persistence.Value = double.PositiveInfinity;
        scope.Intensity.Value = 0.4;

        return scope;
    }
}
