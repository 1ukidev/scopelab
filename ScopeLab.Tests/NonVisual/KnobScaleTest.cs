using NUnit.Framework;
using ScopeLab.Graphics.Controls;
using ScopeLab.Simulation;

namespace ScopeLab.Tests.NonVisual;

[TestFixture]
public class KnobScaleTest
{
    [Test]
    public void TestOneTwoFiveSequence() =>
        Assert.That(ScaleSteps.OneTwoFive(1e-3, 1), Is.EqualTo(new[] { 1e-3, 2e-3, 5e-3, 0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1 }));

    [Test]
    public void TestSteppedScaleClampsAtEnds()
    {
        var scale = new SteppedScale([1, 2, 5]);

        Assert.Multiple(() =>
        {
            Assert.That(scale.Turn(2, 1), Is.EqualTo(5));
            Assert.That(scale.Turn(5, 3), Is.EqualTo(5));
            Assert.That(scale.Turn(2, -4), Is.EqualTo(1));
            Assert.That(scale.AngleFor(1), Is.EqualTo(-KnobScale.SWEEP_ANGLE / 2));
            Assert.That(scale.AngleFor(5), Is.EqualTo(KnobScale.SWEEP_ANGLE / 2));
        });
    }

    [Test]
    public void TestSteppedScaleHandlesSpecialValues()
    {
        var scale = new SteppedScale(Oscilloscope.PERSISTENCE_STEPS);

        Assert.Multiple(() =>
        {
            Assert.That(scale.Turn(0, 1), Is.EqualTo(Oscilloscope.PERSISTENCE_STEPS[1]));
            Assert.That(scale.Turn(double.PositiveInfinity, -1), Is.EqualTo(Oscilloscope.PERSISTENCE_STEPS[^2]));
        });
    }

    [Test]
    public void TestLogScaleRoundsToSignificantDigits()
    {
        var scale = new LogScale(1, 1e6, 20, 3);

        double value = scale.Turn(1000, 0.37);

        Assert.That(value, Is.EqualTo(System.Math.Round(value / 10) * 10));
    }
}
