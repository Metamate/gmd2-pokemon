using System;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon2.Entities;

namespace Pokemon2.World;

// The overworld level: two tilemap layers (base grass + tall grass) and the player entity.
public sealed class Level
{
    public Tilemap BaseLayer  { get; }
    public Tilemap GrassLayer { get; }

    public Player Player { get; }

    public Level(Player player, Tileset tileset)
    {
        Player     = player;
        BaseLayer  = new Tilemap(tileset, GameSettings.MapCols, GameSettings.MapRows);
        GrassLayer = new Tilemap(tileset, GameSettings.MapCols, GameSettings.MapRows);
        GenerateMaps();
    }

    private void GenerateMaps()
    {
        var rng = Random.Shared;

        for (int y = 0; y < GameSettings.MapRows; y++)
        {
            for (int x = 0; x < GameSettings.MapCols; x++)
            {
                int baseId = GameSettings.TileGrass[rng.Next(GameSettings.TileGrass.Length)];
                BaseLayer.SetTile(x, y, new Tile(baseId));

                if (y >= GameSettings.TallGrassStartRow)
                    GrassLayer.SetTile(x, y, new Tile(GameSettings.TileTallGrass));
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        Player.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        BaseLayer.Draw(spriteBatch);
        GrassLayer.Draw(spriteBatch);
        Player.Draw(spriteBatch);
    }
}
