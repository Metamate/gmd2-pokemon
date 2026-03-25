using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.Graphics;

// A 2D grid of integer tile IDs stored row-major.
// ID 0 means empty (not drawn). Non-zero IDs are 1-indexed: tile ID n draws atlas frame n-1.
public sealed class TileMap
{
    private readonly int[,] _ids;

    public int Width  { get; }
    public int Height { get; }

    public TileMap(int width, int height)
    {
        Width  = width;
        Height = height;
        _ids   = new int[height, width];
    }

    public int  GetTile(int x, int y)         => _ids[y, x];
    public void SetTile(int x, int y, int id) => _ids[y, x] = id;

    public void Draw(SpriteBatch spriteBatch, TextureAtlas atlas, int tileSize)
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                int id = _ids[y, x];
                if (id == 0) continue;
                atlas.GetRegion($"frame_{id - 1}")
                     .Draw(spriteBatch, new Vector2(x * tileSize, y * tileSize), Color.White);
            }
    }
}
