using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osuTK;
using ScopeLab.Graphics.Controls;
using ScopeLab.Localisation;

namespace ScopeLab.Tests.Visual;

[TestFixture]
public partial class TestSceneLocalisation : ScopeLabTestScene
{
    [Resolved]
    private FrameworkConfigManager config { get; set; } = null!;

    private LanguageSelector selector = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new FillFlowContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 20),
            Children =
            [
                selector = new LanguageSelector { Width = 120 },
                new ScopeButton(ControlStrings.Default) { Size = new Vector2(140, 40) },
            ],
        });
    }

    [Test]
    public void TestSwitchingLanguage()
    {
        AddStep("select portuguese", () => selector.Current.Value = Language.Portuguese);
        AddAssert("locale stored", () => config.Get<string>(FrameworkSetting.Locale), () => Is.EqualTo("pt"));
        AddUntilStep("button translated", () => displays("PADRÃO"));

        AddStep("select english", () => selector.Current.Value = Language.English);
        AddUntilStep("button back in english", () => displays("DEFAULT"));
    }

    private bool displays(string text) => this.ChildrenOfType<SpriteText>().Any(t => t.Current.Value == text);
}
