using System;
using osu.Framework.Graphics;
using osuTK;
using ScopeLab.Simulation;

namespace ScopeLab.Graphics.Controls;

public partial class SlopeSelector() : OptionSelector<TriggerSlope>(Enum.GetValues<TriggerSlope>())
{
    protected override Drawable CreateContent(TriggerSlope item) => GlyphPath.Edge(item, new Vector2(20, 11));
}
