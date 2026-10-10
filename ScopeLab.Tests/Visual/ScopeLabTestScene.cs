using osu.Framework.Testing;

namespace ScopeLab.Tests.Visual;

public abstract partial class ScopeLabTestScene : TestScene
{
    protected override ITestSceneTestRunner CreateRunner() => new ScopeLabTestSceneTestRunner();

    private partial class ScopeLabTestSceneTestRunner : ScopeLabGameBase, ITestSceneTestRunner
    {
        private TestSceneTestRunner.TestRunner runner = null!;

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            Add(runner = new TestSceneTestRunner.TestRunner());
        }

        public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
    }
}
