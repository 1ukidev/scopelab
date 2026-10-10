using System.Collections.Generic;
using osu.Framework.Graphics;
using osuTK;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// A polyline swept by the electron beam, in the local space of a <see cref="PhosphorLayer"/>.
/// Consecutive points are assumed to be equally spaced in time.
/// </summary>
public class BeamTrace
{
    public readonly List<Vector2> Points = new List<Vector2>();

    public Colour4 Colour = Colour4.White;

    public float Intensity = 1;

    /// <summary>
    /// The distance between consecutive points at which the trace reaches its nominal <see cref="Intensity"/>.
    /// Faster beam movement spreads the same energy over a longer path, so steep edges appear dimmer.
    /// </summary>
    public float ReferenceSpacing = 1;

    /// <summary>
    /// Draws a bright spot at the last point, where the beam currently is.
    /// </summary>
    public bool ShowBeamSpot;
}
