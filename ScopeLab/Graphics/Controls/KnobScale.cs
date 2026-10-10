using System;
using System.Collections.Generic;
using ScopeLab.Simulation;

namespace ScopeLab.Graphics.Controls;

/// <summary>
/// Defines how turning a <see cref="Knob"/> maps onto its value.
/// </summary>
public abstract class KnobScale
{
    public const float SWEEP_ANGLE = 270;

    /// <summary>
    /// Whether the knob has end stops. Bounded knobs display a value arc, unbounded ones behave like endless encoders.
    /// </summary>
    public abstract bool Bounded { get; }

    /// <summary>
    /// Whether the knob only moves in whole detents.
    /// </summary>
    public virtual bool Discrete => false;

    /// <summary>
    /// Pixels of mouse travel per detent.
    /// </summary>
    public virtual float DragPerDetent => 4;

    /// <summary>
    /// Pointer rotation for a value, in degrees clockwise from the top.
    /// </summary>
    public abstract float AngleFor(double value);

    /// <summary>
    /// The position of a value within the knob's travel (0..1). Only meaningful for <see cref="Bounded"/> scales.
    /// </summary>
    public virtual float ProgressFor(double value) => (AngleFor(value) + SWEEP_ANGLE / 2) / SWEEP_ANGLE;

    public abstract double Turn(double value, double detents);

    protected static float AngleForProgress(double progress) => (float)(Math.Clamp(progress, 0, 1) * SWEEP_ANGLE - SWEEP_ANGLE / 2);
}

/// <summary>
/// A rotary switch selecting between fixed positions, such as the 1-2-5 volts/div sequence.
/// </summary>
public class SteppedScale(IReadOnlyList<double> steps) : KnobScale
{
    public override bool Bounded => true;

    public override bool Discrete => true;

    public override float DragPerDetent => 14;

    public override float AngleFor(double value) => AngleForProgress(ScaleSteps.NearestIndex(steps, value) / (double)(steps.Count - 1));

    public override double Turn(double value, double detents) =>
        steps[Math.Clamp(ScaleSteps.NearestIndex(steps, value) + (int)Math.Round(detents), 0, steps.Count - 1)];
}

/// <summary>
/// A continuous logarithmic control, spanning several decades.
/// </summary>
public class LogScale(double min, double max, double detentsPerDecade = 20, int significantDigits = 3) : KnobScale
{
    public override bool Bounded => true;

    public override float AngleFor(double value) => AngleForProgress(Math.Log(value / min) / Math.Log(max / min));

    public override double Turn(double value, double detents)
    {
        double result = Math.Clamp(value * Math.Pow(10, detents / detentsPerDecade), min, max);
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(result)) - significantDigits + 1);

        return Math.Clamp(Math.Round(result / magnitude) * magnitude, min, max);
    }
}

/// <summary>
/// A continuous linear control with end stops.
/// </summary>
public class LinearScale(double min, double max, double stepPerDetent) : KnobScale
{
    public override bool Bounded => true;

    public override float AngleFor(double value) => AngleForProgress((value - min) / (max - min));

    public override double Turn(double value, double detents) => Math.Clamp(value + detents * stepPerDetent, min, max);
}

/// <summary>
/// An endless rotary encoder, as found on modern instruments for position and level controls.
/// </summary>
/// <param name="stepPerDetent">Value change per detent. Evaluated lazily, so it can depend on other settings.</param>
/// <param name="degreesPerDetent">How far the knob visually turns per detent.</param>
public class EncoderScale(Func<double> stepPerDetent, float degreesPerDetent = 12) : KnobScale
{
    public override bool Bounded => false;

    public override float AngleFor(double value) => (float)(value / stepPerDetent() * degreesPerDetent);

    public override double Turn(double value, double detents) => value + detents * stepPerDetent();
}
