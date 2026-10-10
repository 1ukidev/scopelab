using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;

namespace ScopeLab.Graphics.Controls;

/// <summary>
/// A tactile push button, optionally with an indicator LED.
/// </summary>
public partial class ScopeButton : CompositeDrawable, IHasTooltip
{
    public Action? Action { get; set; }

    public LocalisableString TooltipText { get; init; }

    public Colour4 Accent { get; init; } = ScopeColours.TEXT;

    /// <summary>
    /// When set, the button displays an LED that lights up while this is <c>true</c>.
    /// </summary>
    public IBindable<bool>? Lit { get; init; }

    public float FontSize { get; init; } = 13;

    private readonly LocalisableString text;
    private readonly IconUsage? icon;

    private readonly IBindable<bool> lit = new Bindable<bool>();

    private Container body = null!;
    private Box litOverlay = null!;
    private Box hoverOverlay = null!;
    private Box flashOverlay = null!;
    private CircularContainer? led;
    private FillFlowContainer label = null!;

    public ScopeButton(LocalisableString text, IconUsage? icon = null)
    {
        this.text = text;
        this.icon = icon;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        label = new FillFlowContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(6, 0),
        };

        if (Lit != null)
        {
            label.Add(led = new CircularContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Size = new Vector2(7),
                Masking = true,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = Accent.Opacity(0.8f),
                    Radius = 6,
                },
                Child = new Box { RelativeSizeAxes = Axes.Both },
            });
        }

        if (icon is IconUsage iconUsage)
        {
            label.Add(new SpriteIcon
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Icon = iconUsage,
                Size = new Vector2(FontSize * 0.85f),
            });
        }

        if (text != default)
        {
            label.Add(new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Text = text,
                Font = ScopeFont.Condensed(FontSize, true),
            });
        }

        InternalChild = body = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 6,
            BorderThickness = 1,
            BorderColour = ScopeColours.BORDER,
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Colour4.Black.Opacity(0.4f),
                Radius = 4,
                Offset = new Vector2(0, 2),
            },
            Children =
            [
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(ScopeColours.PANEL_LIGHT.Lighten(0.15f), ScopeColours.PANEL_LIGHT),
                },
                litOverlay = new Box { RelativeSizeAxes = Axes.Both, Colour = Accent, Alpha = 0 },
                hoverOverlay = new Box { RelativeSizeAxes = Axes.Both, Colour = Colour4.White, Alpha = 0 },
                flashOverlay = new Box { RelativeSizeAxes = Axes.Both, Colour = Accent, Alpha = 0 },
                label,
            ],
        };

        if (Lit != null)
            lit.BindTo(Lit);
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        lit.BindValueChanged(e =>
        {
            const double duration = 250;

            litOverlay.FadeTo(e.NewValue ? 0.16f : 0, duration, Easing.OutQuint);
            label.FadeColour(e.NewValue ? Accent : ScopeColours.TEXT, duration, Easing.OutQuint);

            if (led != null)
            {
                led.FadeColour(e.NewValue ? Accent : ScopeColours.PANEL_DARK, duration, Easing.OutQuint);
                led.FadeEdgeEffectTo(e.NewValue ? 1 : 0, duration, Easing.OutQuint);
            }
        }, true);

        FinishTransforms(true);
    }

    protected override bool OnHover(HoverEvent e)
    {
        hoverOverlay.FadeTo(0.07f, 200, Easing.OutQuint);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e) => hoverOverlay.FadeOut(300, Easing.OutQuint);

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        body.ScaleTo(0.93f, 200, Easing.OutQuint);
        return true;
    }

    protected override void OnMouseUp(MouseUpEvent e) => body.ScaleTo(1, 600, Easing.OutElastic);

    protected override bool OnClick(ClickEvent e)
    {
        Trigger();
        return true;
    }

    /// <summary>
    /// Activates the button as if it were clicked.
    /// </summary>
    public void Trigger()
    {
        flashOverlay.FadeTo(0.35f).FadeOut(500, Easing.OutQuint);
        Action?.Invoke();
    }
}
