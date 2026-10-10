using System;
using System.Linq;
using NUnit.Framework;
using ScopeLab.Simulation;

namespace ScopeLab.Tests.NonVisual;

[TestFixture]
public class WaveformStatisticsTest
{
    private const double interval = 5e-6;

    [Test]
    public void TestSine()
    {
        var statistics = compute(t => Math.Sin(Math.Tau * 1000 * t));

        Assert.Multiple(() =>
        {
            Assert.That(statistics.Frequency, Is.EqualTo(1000).Within(0.5).Percent);
            Assert.That(statistics.PeakToPeak, Is.EqualTo(2).Within(0.01));
            Assert.That(statistics.Rms, Is.EqualTo(Math.Sqrt(0.5)).Within(0.01));
            Assert.That(statistics.Mean, Is.EqualTo(0).Within(0.01));
            Assert.That(statistics.DutyCycle, Is.EqualTo(0.5).Within(0.02));
        });
    }

    [Test]
    public void TestPulseDutyCycle()
    {
        var statistics = compute(t => Waveforms.Evaluate(WaveformShape.Pulse, t * 1000, 1000));

        Assert.That(statistics.DutyCycle, Is.EqualTo(Waveforms.PULSE_DUTY).Within(0.02));
    }

    [Test]
    public void TestShortCaptureUsesBothEdges()
    {
        // Barely more than one and a half periods: not enough rising edges alone to time a full period.
        var samples = Enumerable.Range(0, 1000).Select(i => Math.Sin(Math.Tau * 1.6 * i / 1000 - 1)).ToArray();
        var statistics = WaveformStatistics.Compute(samples, 1e-3, 0.1);

        Assert.That(statistics.Frequency, Is.EqualTo(1.6).Within(1).Percent);
    }

    [Test]
    public void TestFlatSignalHasNoFrequency()
    {
        var statistics = compute(_ => 1.5);

        Assert.Multiple(() =>
        {
            Assert.That(statistics.Frequency, Is.NaN);
            Assert.That(statistics.Mean, Is.EqualTo(1.5));
        });
    }

    private static WaveformStatistics compute(Func<double, double> signal)
    {
        double[] samples = Enumerable.Range(0, Acquisition.SAMPLE_COUNT).Select(i => signal(i * interval)).ToArray();
        return WaveformStatistics.Compute(samples, interval, 0.1);
    }
}
