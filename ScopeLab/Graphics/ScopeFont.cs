using osu.Framework.Graphics.Sprites;

namespace ScopeLab.Graphics;

public static class ScopeFont
{
    public static FontUsage Regular(float size, bool bold = false) => new FontUsage("Roboto", size, bold ? "Bold" : "Regular");

    public static FontUsage Condensed(float size, bool bold = false) => new FontUsage("RobotoCondensed", size, bold ? "Bold" : "Regular");

    /// <summary>
    /// Fixed-width digits, so readouts don't jitter while values change.
    /// </summary>
    public static FontUsage Numeric(float size, bool bold = true) => Condensed(size, bold).With(fixedWidth: true);
}
