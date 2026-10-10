using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// A compact on-screen readout, which flashes briefly whenever its value changes.
/// </summary>
public partial class StatusBadge : CompositeDrawable
{
    private readonly Box background;
    private readonly Box flash;
    private readonly SpriteText captionText;
    private readonly SpriteText valueText;
    private readonly SpriteIcon icon;

    private Colour4 valueColour = ScopeColours.TEXT;
    private Colour4 captionColour = ScopeColours.TEXT_FAINT;
    private (Colour4 colour, float alpha) fill = (ScopeColours.PANEL_LIGHT, 0.6f);

    public StatusBadge(string caption = "")
    {
        AutoSizeAxes = Axes.Both;

        InternalChild = new Container
        {
            AutoSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 4,
            Children =
            [
                background = new Box { RelativeSizeAxes = Axes.Both, Colour = ScopeColours.PANEL_LIGHT, Alpha = 0.6f },
                flash = new Box { RelativeSizeAxes = Axes.Both, Alpha = 0 },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(5, 0),
                    Padding = new MarginPadding { Horizontal = 7, Vertical = 3 },
                    Children =
                    [
                        captionText = new SpriteText
                        {
                            Text = caption,
                            Font = ScopeFont.Condensed(13, true),
                            Colour = ScopeColours.TEXT_FAINT,
                            Alpha = string.IsNullOrEmpty(caption) ? 0 : 1,
                        },
                        icon = new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Size = new Vector2(10),
                            Alpha = 0,
                        },
                        valueText = new SpriteText
                        {
                            Font = ScopeFont.Numeric(13),
                            Colour = ScopeColours.TEXT,
                        },
                    ],
                },
            ],
        };
    }

    public string Value
    {
        get;
        set
        {
            if (field == value)
                return;

            bool initial = field.Length == 0;

            field = value;
            valueText.Text = value;

            if (!initial)
                flash.FadeTo(0.25f).FadeOut(400, Easing.OutQuint);
        }
    } = string.Empty;

    /// <summary>
    /// An optional glyph shown before the value.
    /// </summary>
    public IconUsage? Icon
    {
        set
        {
            icon.Alpha = value.HasValue ? 1 : 0;

            if (value is IconUsage usage)
                icon.Icon = usage;
        }
    }

    public Colour4 ValueColour
    {
        set
        {
            if (valueColour == value)
                return;

            valueColour = value;
            fadeColour(valueText, value);
            fadeColour(icon, value);
            flash.Colour = value;
        }
    }

    public Colour4 CaptionColour
    {
        set
        {
            if (captionColour == value)
                return;

            captionColour = value;
            fadeColour(captionText, value);
        }
    }

    /// <summary>
    /// Fills the badge with a solid colour, for prominent states like RUN / STOP.
    /// </summary>
    public void SetFill(Colour4 colour, float alpha)
    {
        if (fill == (colour, alpha))
            return;

        fill = (colour, alpha);
        fadeColour(background, colour);
        background.FadeTo(alpha, IsLoaded ? 200 : 0, Easing.OutQuint);
    }

    private void fadeColour(Drawable drawable, Colour4 colour) => drawable.FadeColour(colour, IsLoaded ? 200 : 0, Easing.OutQuint);
}
