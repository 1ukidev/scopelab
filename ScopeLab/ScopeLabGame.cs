using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Screens;
using ScopeLab.Graphics.Containers;

namespace ScopeLab;

public partial class ScopeLabGame : ScopeLabGameBase
{
    private ScreenStack screenStack = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new ScopeTooltipContainer
        {
            RelativeSizeAxes = Axes.Both,
            Child = screenStack = new ScreenStack { RelativeSizeAxes = Axes.Both },
        };
    }

    /// <summary>
    /// Defaults only apply while the setting is absent from framework.ini, so a user's own choice is never overridden.
    /// </summary>
    protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults() => new Dictionary<FrameworkSetting, object>
    {
        [FrameworkSetting.FrameSync] = FrameSync.VSync,
    };

    public override void SetHost(GameHost host)
    {
        base.SetHost(host);

        host.Window?.Title = "ScopeLab";
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        screenStack.Push(new MainScreen());
    }
}
