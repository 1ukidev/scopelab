using System;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using ScopeLab.Graphics;
using ScopeLab.Graphics.Controls;
using ScopeLab.Simulation;
using ScopeLab.Utils;

namespace ScopeLab.Tests.Visual;

[TestFixture]
public partial class TestSceneControls : ScopeLabTestScene
{
    private readonly BindableDouble stepped = new BindableDouble(1);
    private readonly BindableDouble frequency = new BindableDouble(1000);
    private readonly BindableDouble position = new BindableDouble();
    private readonly Bindable<Coupling> coupling = new Bindable<Coupling>();
    private readonly BindableBool lit = new BindableBool();

    public TestSceneControls()
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
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(20, 0),
                    Children =
                    [
                        new Knob("Stepped", new SteppedScale(ScopeChannel.VOLTS_PER_DIVISION), 60)
                        {
                            Current = stepped,
                            Accent = ScopeColours.ForChannel(0),
                            Format = v => SiFormat.Format(v, "V"),
                        },
                        new Knob("Log", new LogScale(1, 1e6), 50)
                        {
                            Current = frequency,
                            Accent = ScopeColours.ForChannel(1),
                            Format = v => SiFormat.Format(v, "Hz"),
                        },
                        new Knob("Encoder", new EncoderScale(() => 0.05), 40)
                        {
                            Current = position,
                            Accent = ScopeColours.TRIGGER,
                        },
                    ],
                },
                new OptionSelector<Coupling>(Enum.GetValues<Coupling>())
                {
                    Width = 220,
                    Current = coupling,
                    Accent = ScopeColours.XY,
                },
                new ScopeButton("TOGGLE")
                {
                    Size = new Vector2(120, 32),
                    Lit = lit,
                    Accent = ScopeColours.RUN,
                    Action = () => lit.Toggle(),
                },
            ],
        });
    }

    [Test]
    public void TestExternalChanges()
    {
        AddStep("set volts/div", () => stepped.Value = 0.05);
        AddStep("set frequency", () => frequency.Value = 250000);
        AddStep("spin encoder", () => position.Value += 3);
        AddStep("select AC", () => coupling.Value = Coupling.AC);
        AddStep("toggle LED", () => lit.Toggle());
    }
}
