using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osuTK;

namespace ScopeLab.Graphics.Controls;

/// <summary>
/// A text link that opens a web page in the system browser.
/// </summary>
public partial class ExternalLink : CompositeDrawable, IHasTooltip
{
    public LocalisableString TooltipText { get; init; }

    [Resolved]
    private GameHost host { get; set; } = null!;

    private readonly string url;
    private readonly FillFlowContainer content;

    public ExternalLink(string url, LocalisableString text, IconUsage icon)
    {
        this.url = url;

        AutoSizeAxes = Axes.Both;

        InternalChild = content = new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(6, 0),
            Colour = ScopeColours.TEXT_DIM,
            Children =
            [
                new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Icon = icon,
                    Size = new Vector2(14),
                },
                new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = text,
                    Font = ScopeFont.Condensed(13, true),
                },
            ],
        };
    }

    protected override bool OnHover(HoverEvent e)
    {
        content.FadeColour(ScopeColours.XY, 200, Easing.OutQuint);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e) => content.FadeColour(ScopeColours.TEXT_DIM, 300, Easing.OutQuint);

    protected override bool OnClick(ClickEvent e)
    {
        host.OpenUrlExternally(url);
        return true;
    }
}
