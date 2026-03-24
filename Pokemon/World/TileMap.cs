using GMDCore.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon.World;

/// <summary>
/// A 2-D grid of Tiles. Equivalent to the Lua TileMap class.
/// Tiles are stored row-major in a 2-D array (0-indexed internally).
/// Grid coordinates passed to GetTile are 1-indexed (matching Lua).
/// </summary>
public sealed class TileMap
{
    private readonly Tile[,] _tiles;

    public int Width  { get; }
    public int Height { get; }

    public TileMap(int width, int height)
    {
        Width  = width;
        Height = height;
        _tiles = new Tile[height, width];
    }

    /// <summary>Get tile at 1-indexed (x, y) grid coordinates.</summary>
    public Tile GetTile(int x, int y) => _tiles[y - 1, x - 1];

    /// <summary>Set tile at 1-indexed (x, y) grid coordinates.</summary>
    public void SetTile(int x, int y, Tile tile) => _tiles[y - 1, x - 1] = tile;

    public void Draw(SpriteBatch spriteBatch, TextureAtlas tileAtlas)
    {
        for (int row = 0; row < Height; row++)
            for (int col = 0; col < Width; col++)
                _tiles[row, col]?.Draw(spriteBatch, tileAtlas);
    }
}
