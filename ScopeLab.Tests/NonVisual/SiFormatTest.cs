using NUnit.Framework;
using ScopeLab.Utils;

namespace ScopeLab.Tests.NonVisual;

[TestFixture]
public class SiFormatTest
{
    [TestCase(0.0002, "s", false, "200 us")]
    [TestCase(1500, "Hz", false, "1.50 kHz")]
    [TestCase(999.96, "Hz", false, "1.00 kHz")]
    [TestCase(-0.5, "V", false, "-500 mV")]
    [TestCase(0.25, "V", true, "+250 mV")]
    [TestCase(0, "V", false, "0.00 V")]
    [TestCase(12.345e6, "Hz", false, "12.3 MHz")]
    public void TestFormat(double value, string unit, bool signed, string expected) =>
        Assert.That(SiFormat.Format(value, unit, signed: signed), Is.EqualTo(expected));

    [Test]
    public void TestNotANumber() => Assert.That(SiFormat.Format(double.NaN, "Hz"), Is.EqualTo("--- Hz"));

    [Test]
    public void TestSignificantDigits() => Assert.That(SiFormat.Format(1234.5678, "Hz", 6), Is.EqualTo("1.23457 kHz"));
}
