using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Testing;
using ScopeLab.Graphics.Display;
using ScopeLab.Simulation;

namespace ScopeLab.Tests.Visual;

[TestFixture]
public partial class TestSceneScopeDisplay : ScopeLabTestScene
{
    [Cached]
    private readonly Oscilloscope scope = new Oscilloscope();

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(scope);
        Add(new ScopeDisplay
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });
    }

    [Test]
    public void TestDisplayFormats()
    {
        AddStep("YT", () => scope.DisplayMode.Value = DisplayMode.YT);
        AddStep("XY", () => scope.DisplayMode.Value = DisplayMode.XY);
        AddStep("detune CH2 for a rotating figure", () => scope.Generators[1].Frequency.Value = 1001);
        AddStep("back to YT", () => scope.DisplayMode.Value = DisplayMode.YT);
    }

    [Test]
    public void TestTimebases()
    {
        AddStep("fast", () => scope.Timebase.SecondsPerDivision.Value = 100e-9);
        AddStep("progressive sweep", () => scope.Timebase.SecondsPerDivision.Value = 50e-3);
        AddStep("roll", () => scope.Timebase.SecondsPerDivision.Value = 0.5);
        AddStep("normal", () => scope.Timebase.SecondsPerDivision.Value = 200e-6);
    }

    [Test]
    public void TestPersistenceAndNoise()
    {
        AddStep("add noise", () => scope.Generators[0].NoiseLevel.Value = 0.1);
        AddStep("infinite persistence", () => scope.Persistence.Value = double.PositiveInfinity);
        AddStep("minimum persistence", () => scope.Persistence.Value = 0);
        AddStep("remove noise", () => scope.Generators[0].NoiseLevel.Value = 0);
    }

    [Test]
    public void TestMathAndSingle()
    {
        AddStep("math add", () => scope.MathFunction.Value = MathMode.Add);
        AddStep("math multiply", () => scope.MathFunction.Value = MathMode.Multiply);
        AddStep("single", scope.Single);
        AddUntilStep("stopped after one sweep", () => !scope.Running.Value);
        AddStep("run", () => scope.Running.Value = true);
        AddStep("math off", () => scope.MathFunction.Value = MathMode.Off);
    }

    [Test]
    public void TestTriggerStatusIsStable()
    {
        int consecutiveTriggered = 0;

        // A slow signal produces fewer trigger events than update frames, which must not read as "waiting".
        AddStep("slow signal", () =>
        {
            scope.Generators[0].Frequency.Value = 100;
            scope.Timebase.SecondsPerDivision.Value = 2e-3;
            scope.Trigger.Mode.Value = TriggerMode.Normal;
        });
        AddUntilStep("triggered for 120 consecutive frames", () =>
        {
            consecutiveTriggered = scope.State.Value == AcquisitionState.Triggered ? consecutiveTriggered + 1 : 0;
            return consecutiveTriggered >= 120;
        });
        AddStep("remove signal", () => scope.Generators[0].Output.Value = false);
        AddUntilStep("waiting", () => scope.State.Value == AcquisitionState.Waiting);
        AddStep("restore", () =>
        {
            scope.Generators[0].Output.Value = true;
            scope.Generators[0].Frequency.SetDefault();
            scope.Timebase.SecondsPerDivision.SetDefault();
            scope.Trigger.Mode.SetDefault();
        });
    }

    [Test]
    public void TestCoincidingMarkersStaySeparate()
    {
        AddStep("place both channels at the centre", () =>
        {
            foreach (var channel in scope.Channels)
                channel.Position.Value = 0;
        });
        // Only the arrow tip of the moved marker may touch the other one, never its label.
        AddUntilStep("labels don't overlap", () =>
        {
            var markers = this.ChildrenOfType<ChannelMarker>().Select(m => m.ScreenSpaceDrawQuad.AABBFloat).ToArray();
            float overlap = RectangleF.Intersect(markers[0], markers[1]).Width;
            return overlap <= markers[0].Width * 0.3f;
        });
        AddStep("restore positions", () =>
        {
            foreach (var channel in scope.Channels)
                channel.Position.SetDefault();
        });
        AddUntilStep("markers back in line", () => this.ChildrenOfType<ChannelMarker>().All(m => Math.Abs(m.X) < 0.5f));
    }

    [Test]
    public void TestAutoSet()
    {
        AddStep("scramble settings", () =>
        {
            scope.Channels[0].VoltsPerDivision.Value = 10e-3;
            scope.Timebase.SecondsPerDivision.Value = 1;
            scope.Trigger.Level.Value = 5;
        });
        AddStep("auto set", scope.AutoSet);
        AddAssert("triggered timebase", () => scope.Timebase.SecondsPerDivision.Value < Timebase.ROLL_THRESHOLD);
        AddUntilStep("triggering", () => scope.State.Value == AcquisitionState.Triggered);
    }
}
