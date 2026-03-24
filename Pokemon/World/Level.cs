using System;
using System.Collections.Generic;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Entities;

namespace Pokemon.World;

/// <summary>
/// The overworld level: two tile layers (base grass + tall grass) and the player entity.
/// Equivalent to the Lua Level class.
/// </summary>
public sealed class Level
{
    private const int LevelWidth  = 50;
    private const int LevelHeight = 50;

    public TileMap BaseLayer  { get; } = new(LevelWidth, LevelHeight);
    public TileMap GrassLayer { get; } = new(LevelWidth, LevelHeight);

    public Player Player { get; }

    private readonly TextureAtlas _tileAtlas;

    public Level(Player player, TextureAtlas tileAtlas)
    {
        Player     = player;
        _tileAtlas = tileAtlas;
        GenerateMaps();
    }

    private void GenerateMaps()
    {
        var rng = Random.Shared;

        for (int y = 1; y <= LevelHeight; y++)
        {
            for (int x = 1; x <= LevelWidth; x++)
            {
                // Base layer: random short-grass tile
                int baseId = GameSettings.TileGrass[rng.Next(GameSettings.TileGrass.Length)];
                BaseLayer.SetTile(x, y, new Tile(x, y, baseId));

                // Grass layer: tall grass below row 10, otherwise empty
                int grassId = y > GameSettings.TallGrassStartRow - 1
                    ? GameSettings.TileTallGrass
                    : GameSettings.TileEmpty;
                GrassLayer.SetTile(x, y, new Tile(x, y, grassId));
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        Player.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        BaseLayer.Draw(spriteBatch, _tileAtlas);
        GrassLayer.Draw(spriteBatch, _tileAtlas);
        Player.Draw(spriteBatch);
    }
}
