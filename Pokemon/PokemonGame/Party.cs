using System.Collections.Generic;

namespace Pokemon.PokemonGame;

// A collection of Pokemon owned by a trainer (player or opponent).
public sealed class Party
{
    private readonly List<PokemonInstance> _pokemon = new();
    public IReadOnlyList<PokemonInstance> Pokemon => _pokemon;

    public Party(IEnumerable<PokemonInstance> pokemon)
    {
        _pokemon.AddRange(pokemon);
    }
}
