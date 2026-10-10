using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osuTK;

namespace ScopeLab.Graphics.Containers;

public partial class ScopeTooltipContainer : TooltipContainer
{
    protected override double AppearDelay => 450;

    protected override ITooltip CreateTooltip() => new ScopeTooltip();

    private partial class ScopeTooltip : VisibilityContainer, ITooltip<LocalisableString>
    {
        private readonly TextFlowContainer text;

        public ScopeTooltip()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 6;
            BorderThickness = 1;
            BorderColour = ScopeColours.BORDER;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Colour4.Black.Opacity(0.5f),
                Radius = 8,
                Offset = new Vector2(0, 3),
            };

            Children =
            [
                new Box { RelativeSizeAxes = Axes.Both, Colour = ScopeColours.PANEL_DARK },
                text = new TextFlowContainer(t =>
                {
                    t.Font = ScopeFont.Regular(13);
                    t.Colour = ScopeColours.TEXT;
                })
                {
                    AutoSizeAxes = Axes.Both,
                    MaximumSize = new Vector2(280, float.MaxValue),
                    Margin = new MarginPadding { Horizontal = 10, Vertical = 7 },
                },
            ];
        }

        public void SetContent(LocalisableString content) => text.Text = content;

        public void Move(Vector2 pos) => Position = pos;

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(150, Easing.OutQuint);
    }
}
