using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;

namespace ScopeLab.Graphics.Controls;

/// <summary>
/// A segmented control with a highlight that glides to the selected option.
/// </summary>
public partial class OptionSelector<T> : CompositeDrawable, IHasTooltip
    where T : notnull
{
    private readonly BindableWithCurrent<T> current = new BindableWithCurrent<T>();

    public Bindable<T> Current
    {
        get => current.Current;
        set => current.Current = value;
    }

    public Colour4 Accent { get; init; } = ScopeColours.TEXT;

    public Func<T, LocalisableString> LabelFor { get; init; } = v => v.ToString()!;

    public LocalisableString TooltipText { get; init; }

    private readonly T[] items;
    private readonly List<Segment> segments = new List<Segment>();

    private Container highlight = null!;

    public OptionSelector(IEnumerable<T> items)
    {
        this.items = items.ToArray();
        Height = 26;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        var segmentContainer = new Container { RelativeSizeAxes = Axes.Both };

        for (int i = 0; i < items.Length; i++)
        {
            var segment = new Segment(items[i], CreateContent(items[i]), Accent)
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.X,
                Width = 1f / items.Length,
                X = (float)i / items.Length,
                Action = v => Current.Value = v,
            };

            segments.Add(segment);
            segmentContainer.Add(segment);
        }

        InternalChild = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 6,
            BorderThickness = 1,
            BorderColour = ScopeColours.BORDER,
            Children =
            [
                new Box { RelativeSizeAxes = Axes.Both, Colour = ScopeColours.PANEL_DARK },
                highlight = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.X,
                    Width = 1f / items.Length,
                    Padding = new MarginPadding(2),
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 4,
                        BorderThickness = 1.5f,
                        BorderColour = Accent.Opacity(0.8f),
                        Child = new Box { RelativeSizeAxes = Axes.Both, Colour = Accent, Alpha = 0.2f },
                    },
                },
                segmentContainer,
            ],
        };
    }

    /// <summary>
    /// Creates the visual representation of an option. Its colour is managed by the selector.
    /// </summary>
    protected virtual Drawable CreateContent(T item) => new SpriteText
    {
        Anchor = Anchor.Centre,
        Origin = Anchor.Centre,
        Text = LabelFor(item),
        Font = ScopeFont.Condensed(13, true),
    };

    protected override void LoadComplete()
    {
        base.LoadComplete();

        Current.BindValueChanged(e =>
        {
            int index = Array.IndexOf(items, e.NewValue);

            highlight.MoveToX((float)index / items.Length, 400, Easing.OutQuint);

            foreach (var segment in segments)
                segment.Selected = EqualityComparer<T>.Default.Equals(segment.Value, e.NewValue);
        }, true);

        FinishTransforms(true);
    }

    private partial class Segment : CompositeDrawable
    {
        public readonly T Value;

        public Action<T>? Action;

        private readonly Drawable content;
        private readonly Colour4 accent;
        private readonly Box hover;
        private bool selected;

        public Segment(T value, Drawable content, Colour4 accent)
        {
            Value = value;
            this.content = content;
            this.accent = accent;

            InternalChildren =
            [
                hover = new Box { RelativeSizeAxes = Axes.Both, Colour = Colour4.White, Alpha = 0 },
                content,
            ];

            content.Colour = ScopeColours.TEXT_DIM;
        }

        public bool Selected
        {
            set
            {
                selected = value;
                updateColour();
            }
        }

        private void updateColour() =>
            content.FadeColour(selected ? accent : IsHovered ? ScopeColours.TEXT : ScopeColours.TEXT_DIM, 250, Easing.OutQuint);

        protected override bool OnHover(HoverEvent e)
        {
            hover.FadeTo(0.05f, 200, Easing.OutQuint);
            updateColour();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hover.FadeOut(300, Easing.OutQuint);
            updateColour();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            content.ScaleTo(0.88f, 200, Easing.OutQuint);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e) => content.ScaleTo(1, 500, Easing.OutElastic);

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke(Value);
            return true;
        }
    }
}
