using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Entities;
using Pokemon.Mons;

namespace Pokemon.Definitions;

// Loads Pokemon battle sprites and entity animations from data files.
public static class EntityDefinitions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly Dictionary<string, Texture2D> _pokemonTextures = new();
    private static EntityAnimationsFile _animationsFile;

    public static void LoadContent(ContentManager content)
    {
        // Load battle sprites for every known species
        foreach (var species in PokemonDefinitions.All)
        {
            _pokemonTextures[species.BattleSpriteFront] = content.Load<Texture2D>(species.BattleSpriteFront);
            _pokemonTextures[species.BattleSpriteBack]  = content.Load<Texture2D>(species.BattleSpriteBack);
        }

        // Load entity animation definitions
        string path = Path.Combine(content.RootDirectory, "data/entity_animations.json");
        using var stream = TitleContainer.OpenStream(path);
        _animationsFile = JsonSerializer.Deserialize<EntityAnimationsFile>(stream, JsonOptions);
    }

    // Retrieve a pre-loaded Pokemon battle sprite by its content path key.
    public static Texture2D GetPokemonSprite(string key)
        => _pokemonTextures.TryGetValue(key, out var tex)
            ? tex
            : throw new KeyNotFoundException($"Pokemon sprite not found: '{key}'");

    // Build the walk + idle animation set from the shared entity atlas.
    public static Dictionary<string, Animation> CreateEntityAnimations(TextureAtlas atlas)
    {
        var animations = new Dictionary<string, Animation>();
        foreach (var entry in _animationsFile.Animations)
            animations[entry.Name] = atlas.CreateAnimation(entry.Frames, _animationsFile.Interval);
        return animations;
    }

    private record EntityAnimationsFile(double Interval, List<AnimationEntry> Animations);
    private record AnimationEntry(string Name, int[] Frames);
}
