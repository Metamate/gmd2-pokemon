using Pokemon.PokemonGame;

namespace Pokemon.Battle;

// The opposing trainer in a battle. Equivalent to the Lua Opponent class.
public sealed class Opponent
{
    public Party Party { get; }

    public Opponent(Party party) => Party = party;

    // Creates a wild opponent with a random Pokemon at a random level.
    public static Opponent CreateWild()
    {
        int level  = System.Random.Shared.Next(GameSettings.OpponentLevelMin, GameSettings.OpponentLevelMax);
        var pokemon = new PokemonInstance(PokemonDefinitions.GetRandom(), level);
        return new Opponent(new Party(new[] { pokemon }));
    }
}
