using GMDCore.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon0;

// Bundles game-specific art and fonts so the rest of the codebase can access
// them through the service locator instead of mixing several global patterns.
public sealed class GameAssets
{
    public TextureAtlas TileAtlas { get; }
    public TextureAtlas EntityAtlas { get; }

    public GameAssets(
        TextureAtlas tileAtlas,
        TextureAtlas entityAtlas)
    {
        TileAtlas = tileAtlas;
        EntityAtlas = entityAtlas;
    }
}
