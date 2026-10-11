using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using ScopeLab.Simulation;

namespace ScopeLab.Tests.Visual;

[TestFixture]
public partial class TestSceneSingleCapture : ScopeLabTestScene
{
    [Cached]
    private readonly Oscilloscope scope = new Oscilloscope();

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(scope);
    }

    [SetUpSteps]
    public void SetUpSteps()
    {
        AddStep("factory setup", scope.ResetToDefaults);
    }

    [Test]
    public void TestCapturesOneSweepAndStops()
    {
        bool armed = false;

        // At fast timebases the sweep completes within a couple of frames, so the armed state is checked right away.
        AddStep("arm single", () =>
        {
            scope.Single();
            armed = scope.SingleArmed.Value && scope.Running.Value;
        });
        AddAssert("was armed and running", () => armed);
        AddUntilStep("stopped after the sweep", () => !scope.Running.Value);
        AddAssert("disarmed", () => !scope.SingleArmed.Value);
        AddAssert("holding a complete sweep", () => scope.State.Value == AcquisitionState.Stopped && scope.Acquisition.Complete);
    }

    [Test]
    public void TestWaitsIndefinitelyWithoutTrigger()
    {
        AddStep("disconnect trigger source", () => scope.Generators[0].Output.Value = false);
        AddStep("arm single", scope.Single);
        AddUntilStep("waiting", () => scope.State.Value == AcquisitionState.Waiting);
        AddWaitStep("let time pass", 30);
        AddAssert("still armed and waiting", () => scope.SingleArmed.Value && scope.Running.Value && scope.State.Value == AcquisitionState.Waiting);
    }

    [Test]
    public void TestRunStopCancels()
    {
        AddStep("disconnect trigger source", () => scope.Generators[0].Output.Value = false);
        AddStep("arm single", scope.Single);
        AddStep("press run/stop", () => scope.Running.Toggle());
        AddAssert("disarmed and stopped", () => !scope.SingleArmed.Value && !scope.Running.Value);
    }

    [Test]
    public void TestStaysArmedDuringSlowSweep()
    {
        AddStep("slow timebase", () => scope.Timebase.SecondsPerDivision.Value = 50e-3);
        AddStep("arm single", scope.Single);
        AddUntilStep("armed while the sweep is drawn", () => scope.SingleArmed.Value && scope.State.Value == AcquisitionState.Triggered && !scope.Acquisition.Complete);
        AddUntilStep("stopped after the sweep", () => !scope.Running.Value);
        AddAssert("disarmed", () => !scope.SingleArmed.Value);
        AddAssert("sweep complete", () => scope.Acquisition.Count, () => Is.EqualTo(Acquisition.SAMPLE_COUNT));
    }

    [Test]
    public void TestDoesNotRollWhileArmed()
    {
        AddStep("roll timebase", () => scope.Timebase.SecondsPerDivision.Value = 0.2);
        AddUntilStep("rolling", () => scope.State.Value == AcquisitionState.Roll);
        AddStep("arm single", scope.Single);
        AddAssert("roll suspended", () => !scope.IsRolling);
        AddUntilStep("captured a triggered sweep", () => !scope.Running.Value);
        AddAssert("not a rolling capture", () => !scope.Acquisition.Rolling);
    }
}
