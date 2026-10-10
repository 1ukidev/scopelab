using System;
using System.Collections.Generic;
using System.Globalization;

namespace ScopeLab.Simulation;

public static class ScaleSteps
{
    /// <summary>
    /// Generates the classic 1-2-5 instrument sequence between two decades (inclusive).
    /// </summary>
    public static double[] OneTwoFive(double min, double max)
    {
        var steps = new List<double>();

        for (double decade = min; decade <= max * 1.001; decade *= 10)
        {
            foreach (double mantissa in (ReadOnlySpan<double>)[1, 2, 5])
            {
                double value = double.Parse((decade * mantissa).ToString("G3", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

                if (value <= max * 1.001)
                    steps.Add(value);
            }
        }

        return steps.ToArray();
    }

    /// <summary>
    /// Finds the step closest to a value, comparing ratios rather than differences.
    /// </summary>
    public static int NearestIndex(IReadOnlyList<double> steps, double value)
    {
        int best = 0;
        double bestDistance = double.MaxValue;

        for (int i = 0; i < steps.Count; i++)
        {
            if (steps[i] == value)
                return i;

            double distance = Math.Abs(Math.Log(steps[i] / value));

            if (distance < bestDistance)
            {
                best = i;
                bestDistance = distance;
            }
        }

        return best;
    }
}
