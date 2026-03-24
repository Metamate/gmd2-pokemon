using Pokemon.PokemonGame;

namespace Pokemon.Entities;

/// <summary>
/// The player-controlled entity. Extends Entity by adding a Pokemon Party.
/// Equivalent to the Lua Player class.
/// </summary>
public sealed class Player : Entity
{
    public Party Party { get; }

    public Player()
    {
        Party = new Party(new[]
        {
            new PokemonInstance(PokemonDefinitions.GetRandom(), GameSettings.PlayerStartLevel)
        });
    }
}
