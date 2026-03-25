using System;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Entities;

namespace Pokemon.World;

// The overworld level: two tile layers (base grass + tall grass) and the player entity.
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

        for (int y = 0; y < LevelHeight; y++)
        {
            for (int x = 0; x < LevelWidth; x++)
            {
                int baseId = GameSettings.TileGrass[rng.Next(GameSettings.TileGrass.Length)];
                BaseLayer.SetTile(x, y, baseId);

                int grassId = y >= GameSettings.TallGrassStartRow ? GameSettings.TileTallGrass : 0;
                GrassLayer.SetTile(x, y, grassId);
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        Player.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        BaseLayer.Draw(spriteBatch, _tileAtlas, GameSettings.TileSize);
        GrassLayer.Draw(spriteBatch, _tileAtlas, GameSettings.TileSize);
        Player.Draw(spriteBatch);
    }
}
