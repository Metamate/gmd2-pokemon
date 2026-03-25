using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Pokemon.Mons;

// Registry of all Mon species, loaded from data/pokemon_definitions.json.
public static class PokemonDefinitions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static IReadOnlyList<PokemonSpecies> All { get; private set; }

    public static void LoadContent(ContentManager content)
    {
        string path = Path.Combine(content.RootDirectory, "data/pokemon_definitions.json");
        using var stream = TitleContainer.OpenStream(path);
        All = JsonSerializer.Deserialize<List<PokemonSpecies>>(stream, JsonOptions);
    }

    // Returns a random species.
    public static PokemonSpecies GetRandom()
        => All[Random.Shared.Next(All.Count)];
}
