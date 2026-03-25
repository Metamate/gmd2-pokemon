using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GMDCore.Graphics;

namespace Pokemon.World;

// A single tile on the map.
public sealed class Tile
{
    public int GridX { get; }
    public int GridY { get; }

    // Tile ID used to look up the sprite from the tilesheet atlas (1-indexed in map data, atlas frames are 0-indexed).
    public int Id { get; }

    public Tile(int gridX, int gridY, int id)
    {
        GridX = gridX;
        GridY = gridY;
        Id    = id;
    }

    public void Draw(SpriteBatch spriteBatch, TextureAtlas tileAtlas)
    {
        if (Id <= 0 || Id == GameSettings.TileEmpty) return;
        var region = tileAtlas.GetRegion($"frame_{Id - 1}");
        region.Draw(spriteBatch, new Vector2(GridX * GameSettings.TileSize,
                                             GridY * GameSettings.TileSize), Color.White);
    }
}
