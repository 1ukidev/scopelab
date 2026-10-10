using System;
using NUnit.Framework;
using ScopeLab.Simulation;

namespace ScopeLab.Tests.NonVisual;

[TestFixture]
public class FrequencyCounterTest
{
    [TestCase(1.5)]
    [TestCase(1234.5678)]
    [TestCase(9.87654e6)]
    public void TestPreciseReading(double frequency)
    {
        double measured = FrequencyCounter.Measure(t => Math.Sin(Math.Tau * frequency * t), 0, 100, 0.1, frequency * 1.05);

        Assert.That(measured, Is.EqualTo(frequency).Within(1e-6).Percent);
    }

    [Test]
    public void TestFlatSignal() => Assert.That(FrequencyCounter.Measure(_ => 0.2, 0, 10, 0.1, 1000), Is.NaN);
}
