using System;
using System.Collections.Generic;
using GMDCore.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Entities;

namespace Pokemon.Definitions;

// Loads and provides Pokemon battle sprite textures.
// Entity walk/idle animations are built from the shared EntityAtlas at runtime.
public static class EntityDefinitions
{
    private static readonly Dictionary<string, Texture2D> _pokemonTextures = new();

    public static void LoadContent(ContentManager content)
    {
        string[] keys =
        {
            "images/pokemon/aardart-front",  "images/pokemon/aardart-back",
            "images/pokemon/agnite-front",   "images/pokemon/agnite-back",
            "images/pokemon/anoleaf-front",  "images/pokemon/anoleaf-back",
            "images/pokemon/bamboon-front",  "images/pokemon/bamboon-back",
            "images/pokemon/cardiwing-front","images/pokemon/cardiwing-back"
        };

        foreach (var key in keys)
            _pokemonTextures[key] = content.Load<Texture2D>(key);
    }

    // Retrieve a pre-loaded Pokemon battle sprite by its content path key.
    public static Texture2D GetPokemonSprite(string key)
        => _pokemonTextures.TryGetValue(key, out var tex) ? tex : null;

    // ---- Entity walk/idle animations ----

    // Build the standard walk + idle animation set from the shared entity atlas.
    // All player and NPC entities share the same sprite sheet.
    public static Dictionary<string, Animation> CreateEntityAnimations(TextureAtlas atlas)
    {
        return new Dictionary<string, Animation>
        {
            [AnimationKeys.WalkDown]  = atlas.CreateAnimation(new[] {3, 4, 5, 4},    GameSettings.WalkAnimIntervalSeconds),
            [AnimationKeys.WalkUp]    = atlas.CreateAnimation(new[] {39, 40, 41, 40}, GameSettings.WalkAnimIntervalSeconds),
            [AnimationKeys.WalkLeft]  = atlas.CreateAnimation(new[] {15, 16, 17, 16}, GameSettings.WalkAnimIntervalSeconds),
            [AnimationKeys.WalkRight] = atlas.CreateAnimation(new[] {27, 28, 29, 28}, GameSettings.WalkAnimIntervalSeconds),
            [AnimationKeys.IdleDown]  = atlas.CreateAnimation(new[] {4},  GameSettings.WalkAnimIntervalSeconds),
            [AnimationKeys.IdleUp]    = atlas.CreateAnimation(new[] {40}, GameSettings.WalkAnimIntervalSeconds),
            [AnimationKeys.IdleLeft]  = atlas.CreateAnimation(new[] {16}, GameSettings.WalkAnimIntervalSeconds),
            [AnimationKeys.IdleRight] = atlas.CreateAnimation(new[] {28}, GameSettings.WalkAnimIntervalSeconds),
        };
    }
}
