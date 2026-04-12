using GMDCore.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon;

// Bundles game-specific art and fonts so the rest of the codebase can access
// them through the service locator instead of mixing several global patterns.
public sealed class GameAssets
{
    public BitmapFont SmallFont { get; }
    public BitmapFont MediumFont { get; }
    public BitmapFont LargeFont { get; }

    public TextureAtlas TileAtlas { get; }
    public TextureAtlas EntityAtlas { get; }
    public Texture2D CursorTex { get; }
    public Texture2D ShadowTex { get; }

    public GameAssets(
        BitmapFont smallFont,
        BitmapFont mediumFont,
        BitmapFont largeFont,
        TextureAtlas tileAtlas,
        TextureAtlas entityAtlas,
        Texture2D cursorTex,
        Texture2D shadowTex)
    {
        SmallFont = smallFont;
        MediumFont = mediumFont;
        LargeFont = largeFont;
        TileAtlas = tileAtlas;
        EntityAtlas = entityAtlas;
        CursorTex = cursorTex;
        ShadowTex = shadowTex;
    }
}
