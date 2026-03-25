using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
    private static readonly Dictionary<string, Texture2D> _pokemonTextures = new();

    public static void LoadContent(ContentManager content)
    {
        // Load battle sprites for every known species
        foreach (var species in PokemonDefinitions.All)
        {
            _pokemonTextures[species.BattleSpriteFront] = content.Load<Texture2D>(species.BattleSpriteFront);
            _pokemonTextures[species.BattleSpriteBack]  = content.Load<Texture2D>(species.BattleSpriteBack);
        }

        // Load entity animation definitions
        string path = Path.Combine(content.RootDirectory, "data/entity_animations.xml");
        using var stream = TitleContainer.OpenStream(path);
        _entityAnimationsDoc = XDocument.Load(stream);
    }

    // Retrieve a pre-loaded Pokemon battle sprite by its content path key.
    public static Texture2D GetPokemonSprite(string key)
        => _pokemonTextures.TryGetValue(key, out var tex)
            ? tex
            : throw new KeyNotFoundException($"Pokemon sprite not found: '{key}'");

    // Build the walk + idle animation set from the shared entity atlas.
    public static Dictionary<string, Animation> CreateEntityAnimations(TextureAtlas atlas)
    {
        var root     = _entityAnimationsDoc.Root;
        double interval = double.Parse(root.Attribute("interval").Value);
        var animations  = new Dictionary<string, Animation>();

        foreach (var el in root.Elements("Animation"))
        {
            string name    = el.Attribute("name").Value;
            int[]  frames  = el.Attribute("frames").Value
                               .Split(',')
                               .Select(int.Parse)
                               .ToArray();
            animations[name] = atlas.CreateAnimation(frames, interval);
        }

        return animations;
    }

    private static XDocument _entityAnimationsDoc;
}
