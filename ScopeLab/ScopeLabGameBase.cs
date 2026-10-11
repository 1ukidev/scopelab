using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osuTK;
using ScopeLab.Localisation;
using ScopeLab.Resources;

namespace ScopeLab;

/// <summary>
/// Shared between the game and the visual test browser.
/// </summary>
public partial class ScopeLabGameBase : osu.Framework.Game
{
    /// <summary>
    /// The resolution the bench layout is designed for. Content scales uniformly to fit the window.
    /// </summary>
    public static readonly Vector2 TARGET_SIZE = new Vector2(1366, 768);

    protected override Container<Drawable> Content { get; }

    protected ScopeLabGameBase()
    {
        base.Content.Add(Content = new DrawSizePreservingFillContainer { TargetDrawSize = TARGET_SIZE });
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Resources.AddStore(new DllResourceStore(typeof(ScopeLabResources).Assembly));

        // The first language registered is the fallback when the system language isn't available.
        foreach (var language in Enum.GetValues<Language>())
            Localisation.AddLanguage(language.ToLocale(), new ScopeLocalisationStore(language));
    }
}
