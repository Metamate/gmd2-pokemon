using System.Collections.Generic;

namespace Pokemon.PokemonGame;

/// <summary>
/// A collection of Pokemon owned by a trainer (player or opponent).
/// Equivalent to the Lua Party class.
/// </summary>
public sealed class Party
{
    public List<PokemonInstance> Pokemon { get; } = new();

    public Party(IEnumerable<PokemonInstance> pokemon)
    {
        Pokemon.AddRange(pokemon);
    }
}
