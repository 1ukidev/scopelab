using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Input;

namespace ScopeLab.Graphics.Controls;

/// <summary>
/// A rotary control. Drag vertically or horizontally, scroll, or double-click to reset.
/// </summary>
public partial class Knob : CompositeDrawable, IHasTooltip
{
    private const float label_height = 15;
    private const float value_height = 17;
    private const float fine_adjustment = 0.15f;

    private readonly BindableWithCurrent<double> current = new BindableWithCurrent<double>();

    public Bindable<double> Current
    {
        get => current.Current;
        set => current.Current = value;
    }

    public Func<double, LocalisableString> Format { get; init; } = v => v.ToString("0.###", CultureInfo.InvariantCulture);

    public LocalisableString TooltipText { get; init; }

    public Colour4 Accent { get; init; } = ScopeColours.TEXT;

    private readonly KnobScale scale;
    private readonly LocalisableString label;
    private readonly float knobSize;

    private Container knob = null!;
    private Container rotor = null!;
    private Container pointer = null!;
    private CircularProgress? valueArc;
    private SpriteText labelText = null!;
    private SpriteText valueText = null!;

    private double dragRemainder;
    private double scrollRemainder;
    private float targetAngle;

    public Knob(LocalisableString label, KnobScale scale, float size = 46)
    {
        this.label = label;
        this.scale = scale;
        knobSize = size;

        Size = new Vector2(Math.Max(size + 18, 62), label_height + size + value_height + 8);
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren =
        [
            labelText = new SpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Text = label.ToUpper(),
                Font = ScopeFont.Condensed(12, true),
                Colour = ScopeColours.TEXT_DIM,
            },
            knob = new Container
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.Centre,
                Y = label_height + 4 + knobSize / 2,
                Size = new Vector2(knobSize),
                Children =
                [
                    createTrack(),
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.76f),
                        Masking = true,
                        CornerRadius = knobSize * 0.38f,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Colour4.Black.Opacity(0.6f),
                            Radius = 6,
                            Offset = new Vector2(0, 3),
                        },
                        Children =
                        [
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourInfo.GradientVertical(ScopeColours.KNOB.Lighten(0.25f), ScopeColours.KNOB_DARK),
                            },
                            new Circle
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.78f),
                                Colour = ColourInfo.GradientVertical(ScopeColours.KNOB_DARK, ScopeColours.KNOB),
                            },
                        ],
                    },
                    rotor = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(0.76f),
                        Children = [createGrip(), pointer = createPointer()],
                    },
                ],
            },
            valueText = new SpriteText
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Font = ScopeFont.Numeric(14),
                Colour = ScopeColours.TEXT,
            },
        ];
    }

    private Drawable createTrack()
    {
        if (!scale.Bounded)
        {
            return new CircularContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderThickness = 1.5f,
                BorderColour = ScopeColours.BORDER,
                Child = new Box { RelativeSizeAxes = Axes.Both, Alpha = 0, AlwaysPresent = true },
            };
        }

        return new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Rotation = -KnobScale.SWEEP_ANGLE / 2,
            Children =
            [
                new CircularProgress
                {
                    RelativeSizeAxes = Axes.Both,
                    InnerRadius = 0.09f,
                    Progress = KnobScale.SWEEP_ANGLE / 360,
                    RoundedCaps = true,
                    Colour = ScopeColours.PANEL_DARK,
                },
                valueArc = new CircularProgress
                {
                    RelativeSizeAxes = Axes.Both,
                    InnerRadius = 0.09f,
                    RoundedCaps = true,
                    Colour = Accent,
                },
            ],
        };
    }

    private Container createPointer() => new Container
    {
        Anchor = Anchor.TopCentre,
        Origin = Anchor.TopCentre,
        Y = knobSize * 0.06f,
        Size = new Vector2(3, knobSize * 0.24f),
        Masking = true,
        CornerRadius = 1.5f,
        EdgeEffect = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Glow,
            Colour = Accent.Opacity(0.35f),
            Radius = 5,
        },
        Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Accent },
    };

    private Container createGrip()
    {
        var grip = new Container { RelativeSizeAxes = Axes.Both };

        const int notches = 20;

        for (int i = 0; i < notches; i++)
        {
            grip.Add(new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Rotation = 360f / notches * i,
                Child = new Box
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(1.5f, knobSize * 0.06f),
                    Y = 1,
                    Colour = Colour4.Black.Opacity(0.35f),
                },
            });
        }

        return grip;
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        targetAngle = scale.AngleFor(Current.Value);
        Current.BindValueChanged(v => updateDisplay(v.NewValue, 250), true);
    }

    private void updateDisplay(double value, double duration)
    {
        valueText.Text = Format(value);

        float angle = scale.AngleFor(value);

        if (!float.IsFinite(angle))
            return;

        // Endless encoders take the shortest way round, so large jumps don't cause dizzying spins.
        targetAngle = scale.Bounded ? angle : targetAngle + wrapDegrees(angle - targetAngle);

        rotor.RotateTo(targetAngle, duration, Easing.OutQuint);
        valueArc?.ProgressTo(scale.ProgressFor(value) * KnobScale.SWEEP_ANGLE / 360, duration, Easing.OutQuint);
    }

    private static float wrapDegrees(float angle) => angle - 360 * MathF.Round(angle / 360);

    private void turn(double detents)
    {
        if (Current.Disabled || detents == 0)
            return;

        double previous = Current.Value;
        Current.Value = scale.Turn(previous, detents);

        if (scale.Discrete && Current.Value != previous)
            pointer.FlashColour(Colour4.White, 300, Easing.OutQuint);
    }

    protected override bool OnHover(HoverEvent e)
    {
        knob.ScaleTo(1.06f, 300, Easing.OutQuint);
        labelText.FadeColour(Accent, 200, Easing.OutQuint);
        return true;
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        if (!IsDragged)
            resetHighlight();
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        knob.ScaleTo(1.02f, 150, Easing.OutQuint);
        return base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        knob.ScaleTo(IsHovered ? 1.06f : 1, 400, Easing.OutElasticHalf);
        base.OnMouseUp(e);
    }

    protected override bool OnDragStart(DragStartEvent e)
    {
        if (e.Button != MouseButton.Left)
            return false;

        dragRemainder = 0;
        valueText.FadeColour(Accent, 150, Easing.OutQuint);
        return true;
    }

    protected override void OnDrag(DragEvent e)
    {
        double detents = (e.Delta.X - e.Delta.Y) / scale.DragPerDetent * (e.ShiftPressed ? fine_adjustment : 1);
        applyDetents(detents, ref dragRemainder);
    }

    protected override void OnDragEnd(DragEndEvent e)
    {
        valueText.FadeColour(ScopeColours.TEXT, 400, Easing.OutQuint);

        if (!IsHovered)
            resetHighlight();
    }

    protected override bool OnScroll(ScrollEvent e)
    {
        applyDetents(e.ScrollDelta.Y * (e.ShiftPressed && !scale.Discrete ? fine_adjustment : 1), ref scrollRemainder);
        return true;
    }

    // The framework only delivers double-clicks to whoever handled the first click.
    protected override bool OnClick(ClickEvent e) => true;

    protected override bool OnDoubleClick(DoubleClickEvent e)
    {
        if (!Current.Disabled)
            Current.SetDefault();

        return true;
    }

    private void applyDetents(double detents, ref double remainder)
    {
        if (!scale.Discrete)
        {
            turn(detents);
            return;
        }

        remainder += detents;
        double whole = Math.Truncate(remainder);
        remainder -= whole;
        turn(whole);
    }

    private void resetHighlight()
    {
        knob.ScaleTo(1, 300, Easing.OutQuint);
        labelText.FadeColour(ScopeColours.TEXT_DIM, 300, Easing.OutQuint);
    }
}
