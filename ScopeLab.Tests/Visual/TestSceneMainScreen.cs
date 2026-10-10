using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;

namespace ScopeLab.Tests.Visual;

[TestFixture]
public partial class TestSceneMainScreen : ScopeLabTestScene
{
    public TestSceneMainScreen()
    {
        Add(new ScreenStack(new MainScreen()) { RelativeSizeAxes = Axes.Both });
    }
}
