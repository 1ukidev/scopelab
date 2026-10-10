using NUnit.Framework;
using osu.Framework.Allocation;

namespace ScopeLab.Tests.Visual;

[TestFixture]
public partial class TestSceneScopeLabGame : ScopeLabTestScene
{
    [BackgroundDependencyLoader]
    private void load()
    {
        AddGame(new ScopeLabGame());
    }
}
