using GMDCore.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon0;

// Bundles game-specific art and fonts so the rest of the codebase can access
// them through the service locator instead of mixing several global patterns.
public sealed class GameAssets
{
    public Tileset Tileset { get; }
    public TextureAtlas EntityAtlas { get; }

    public GameAssets(
        Tileset tileset,
        TextureAtlas entityAtlas)
    {
        Tileset = tileset;
        EntityAtlas = entityAtlas;
    }
}
