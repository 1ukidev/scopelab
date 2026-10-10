using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace ScopeLab.Graphics;

/// <summary>
/// The application icon: a phosphor screen showing a square wave.
/// </summary>
public partial class ScopeLabLogo : Sprite
{
    [BackgroundDependencyLoader]
    private void load(TextureStore textures)
    {
        Texture = textures.Get("logo");
        FillMode = FillMode.Fit;
    }
}
