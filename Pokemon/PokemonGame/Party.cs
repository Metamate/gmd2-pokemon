using System.Collections.Generic;

namespace Pokemon.PokemonGame;

/// <summary>
/// A collection of Pokemon owned by a trainer (player or opponent).
/// Equivalent to the Lua Party class.
/// </summary>
public sealed class Party
{
    private readonly List<PokemonInstance> _pokemon = new();
    public IReadOnlyList<PokemonInstance> Pokemon => _pokemon;

    public Party(IEnumerable<PokemonInstance> pokemon)
    {
        _pokemon.AddRange(pokemon);
    }
}
