using GMDCore.Graphics;
using Pokemon1.Definitions;

namespace Pokemon1.Entities;

// The player-controlled entity. Extends Entity by handling its own
// initialization (start position, size, animations).
public sealed class Player : Entity
{

    public Player(TextureAtlas entityAtlas)
    {
        MapX   = GameSettings.PlayerStartMapX;
        MapY   = GameSettings.PlayerStartMapY;
        Width  = GameSettings.TileSize;
        Height = GameSettings.TileSize;
        X      = MapX * GameSettings.TileSize;
        // Offset Y so the sprite visually stands on the tile, not above it
        Y      = MapY * GameSettings.TileSize - Height / 2f;

        foreach (var (key, anim) in ContentLoader.CreateEntityAnimations(entityAtlas))
            Animations[key] = anim;
    }
}
