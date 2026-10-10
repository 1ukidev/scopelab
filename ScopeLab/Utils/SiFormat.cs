using System;
using System.Globalization;

namespace ScopeLab.Utils;

/// <summary>
/// Formats quantities using engineering notation with SI prefixes (e.g. "2.50 ms", "1.00 kHz").
/// </summary>
public static class SiFormat
{
    // The bundled fonts lack "µ", so "u" is used as many instruments do.
    private static readonly string[] prefixes = ["p", "n", "u", "m", "", "k", "M", "G"];

    private const int unit_prefix_index = 4;

    public static string Format(double value, string unit, int significantDigits = 3, bool signed = false)
    {
        if (double.IsNaN(value))
            return $"--- {unit}";

        if (double.IsInfinity(value))
            return "∞";

        string sign = value < 0 ? "-" : signed ? "+" : "";
        double magnitude = Math.Abs(value);

        if (magnitude < 1e-12)
            return $"{sign}{0.ToString($"F{significantDigits - 1}", CultureInfo.InvariantCulture)} {unit}";

        int exponent = Math.Clamp((int)Math.Floor(Math.Log10(magnitude) / 3), -unit_prefix_index, prefixes.Length - unit_prefix_index - 1);
        double scaled = magnitude / Math.Pow(1000, exponent);

        int integerDigits = scaled >= 100 ? 3 : scaled >= 10 ? 2 : 1;
        int decimals = Math.Max(0, significantDigits - integerDigits);

        scaled = Math.Round(scaled, decimals);

        if (scaled >= 1000 && exponent + unit_prefix_index < prefixes.Length - 1)
        {
            scaled /= 1000;
            exponent++;
            decimals = Math.Max(0, significantDigits - 1);
        }

        return $"{sign}{scaled.ToString($"F{decimals}", CultureInfo.InvariantCulture)} {prefixes[exponent + unit_prefix_index]}{unit}";
    }
}
