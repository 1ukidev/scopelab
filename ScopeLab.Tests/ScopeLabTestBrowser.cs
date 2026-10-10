using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Platform;
using osu.Framework.Testing;

namespace ScopeLab.Tests;

public partial class ScopeLabTestBrowser : ScopeLabGameBase
{
    protected override void LoadComplete()
    {
        base.LoadComplete();

        AddRange(new Drawable[]
        {
            new TestBrowser("ScopeLab"),
            new CursorContainer(),
        });
    }

    public override void SetHost(GameHost host)
    {
        base.SetHost(host);
        host.Window.CursorState |= CursorState.Hidden;
    }
}
