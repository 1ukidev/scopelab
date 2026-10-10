using System;

namespace ScopeLab.Simulation;

/// <summary>
/// Gaussian noise that is a pure function of time, so the same instant always yields the same value.
/// This keeps scrolling (roll mode) and re-sampled data coherent.
/// </summary>
public static class Noise
{
    private const double time_resolution = 1e10;

    public static double Gaussian(double time, ulong seed)
    {
        ulong h1 = mix((ulong)(long)Math.Round(time * time_resolution) ^ seed);
        ulong h2 = mix(h1);

        double u1 = ((h1 >> 11) + 1) * (1.0 / (1UL << 53));
        double u2 = (h2 >> 11) * (1.0 / (1UL << 53));

        return Math.Sqrt(-2 * Math.Log(u1)) * Math.Cos(Math.Tau * u2);
    }

    /// <summary>
    /// A uniformly distributed value in [0, 1) derived from an integer key.
    /// </summary>
    public static double Uniform(long key, ulong seed) => (mix((ulong)key ^ seed) >> 11) * (1.0 / (1UL << 53));

    // SplitMix64 finaliser.
    private static ulong mix(ulong x)
    {
        x += 0x9E3779B97F4A7C15;
        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9;
        x = (x ^ (x >> 27)) * 0x94D049BB133111EB;
        return x ^ (x >> 31);
    }
}
