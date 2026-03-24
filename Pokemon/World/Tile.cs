using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GMDCore.Graphics;

namespace Pokemon.World;

/// <summary>
/// A single tile on the map. Stores its 1-indexed grid position and tile ID.
/// Equivalent to the Lua Tile class.
/// </summary>
public sealed class Tile
{
    // 1-indexed grid coordinates (matching Lua convention)
    public int GridX { get; }
    public int GridY { get; }

    /// <summary>1-indexed tile ID used to look up the sprite from the tilesheet atlas.</summary>
    public int Id { get; }

    public Tile(int gridX, int gridY, int id)
    {
        GridX = gridX;
        GridY = gridY;
        Id    = id;
    }

    public void Draw(SpriteBatch spriteBatch, TextureAtlas tileAtlas)
    {
        // Skip the empty tile (Lua TileEmpty = 101) — it has no visual representation.
        if (Id <= 0 || Id == GameSettings.TileEmpty) return;
        var region = tileAtlas.GetRegion($"frame_{Id - 1}");  // convert 1-indexed → 0-indexed
        region.Draw(spriteBatch, new Vector2((GridX - 1) * GameSettings.TileSize,
                                             (GridY - 1) * GameSettings.TileSize), Color.White);
    }
}
