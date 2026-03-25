using GMDCore.Graphics;
using Pokemon.Definitions;
using Pokemon.PokemonGame;

namespace Pokemon.Entities;

/// <summary>
/// The player-controlled entity. Extends Entity by adding a Pokemon Party
/// and handling its own initialization (start position, size, animations).
/// </summary>
public sealed class Player : Entity
{
    public Party Party { get; }

    public Player(TextureAtlas entityAtlas)
    {
        MapX   = GameSettings.PlayerStartMapX;
        MapY   = GameSettings.PlayerStartMapY;
        Width  = GameSettings.TileSize;
        Height = GameSettings.TileSize;
        X      = (MapX - 1) * GameSettings.TileSize;
        Y      = (MapY - 1) * GameSettings.TileSize - Height / 2f;

        foreach (var (key, anim) in EntityDefinitions.CreateEntityAnimations(entityAtlas))
            Animations[key] = anim;

        Party = new Party(new[]
        {
            new PokemonInstance(PokemonDefinitions.GetRandom(), GameSettings.PlayerStartLevel)
        });
    }
}
