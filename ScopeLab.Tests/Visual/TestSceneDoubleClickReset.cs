using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Testing.Input;
using osuTK;
using osuTK.Input;
using ScopeLab.Graphics.Controls;
using ScopeLab.Graphics.Display;
using ScopeLab.Simulation;

namespace ScopeLab.Tests.Visual;

[TestFixture]
public partial class TestSceneDoubleClickReset : ScopeLabTestScene
{
    [Cached]
    private readonly Oscilloscope scope = new Oscilloscope();

    private readonly BindableDouble value = new BindableDouble(1);

    private ManualInputManager input = null!;
    private Knob knob = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(scope);
        Add(input = new ManualInputManager
        {
            UseParentInput = false,
            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(40, 0),
                Children =
                [
                    knob = new Knob("Test", new LinearScale(0, 10, 0.1), 60) { Current = value },
                    new ScopeDisplay(),
                ],
            },
        });
    }

    [Test]
    public void TestKnobDoubleClickRestoresDefault()
    {
        AddStep("change value", () => value.Value = 7);
        AddStep("move to knob", () => input.MoveMouseTo(knob));
        doubleClick();
        AddAssert("value restored", () => value.Value, () => Is.EqualTo(1));
    }

    [Test]
    public void TestMarkerDoubleClickRestoresDefault()
    {
        AddStep("move channel", () => scope.Channels[0].Position.Value = -3);
        AddUntilStep("marker settled", () => marker.Alpha > 0.9f);
        AddStep("move to marker", () => input.MoveMouseTo(marker));
        doubleClick();
        AddAssert("position restored", () => scope.Channels[0].Position.Value, () => Is.EqualTo(scope.Channels[0].Position.Default));
    }

    private ChannelMarker marker => this.ChildrenOfType<ChannelMarker>().First();

    private void doubleClick()
    {
        AddStep("click", () => input.Click(MouseButton.Left));
        AddStep("click again", () => input.Click(MouseButton.Left));
    }
}
