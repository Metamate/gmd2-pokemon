using GMDCore.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon.World;

// A 2-D grid of Tiles stored row-major.
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

    public Tile GetTile(int x, int y) => _tiles[y, x];

    public void SetTile(int x, int y, Tile tile) => _tiles[y, x] = tile;

    public void Draw(SpriteBatch spriteBatch, TextureAtlas tileAtlas)
    {
        for (int row = 0; row < Height; row++)
            for (int col = 0; col < Width; col++)
                _tiles[row, col]?.Draw(spriteBatch, tileAtlas);
    }
}
