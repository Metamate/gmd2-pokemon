using System;
using System.Collections.Generic;

namespace Pokemon.Mons;

// A collection of Pokemon owned by a trainer (player or opponent).
public sealed class Party
{
    private readonly List<Mon> _pokemon = new();
    public IReadOnlyList<Mon> Pokemon => _pokemon;
    public Mon Current => _pokemon.Count > 0 ? _pokemon[0] : throw new InvalidOperationException("Party is empty.");

    public Party(IEnumerable<Mon> pokemon)
    {
        _pokemon.AddRange(pokemon);
    }
}
