namespace Pokemon.Mons;

// Represents an attack or action a Pokemon can perform in battle.
// Extracts the hardcoded animation timings and names into a data object.
public sealed class Move
{
    public string Name { get; }
    public float LungeDuration { get; }
    public float LungeDistance { get; }
    public float PauseBeforeAttack { get; }
    public int BasePower { get; }

    public Move(string name, int basePower, float lungeDuration, float lungeDistance, float pauseBeforeAttack)
    {
        Name = name;
        BasePower = basePower;
        LungeDuration = lungeDuration;
        LungeDistance = lungeDistance;
        PauseBeforeAttack = pauseBeforeAttack;
    }

    // For now, all Pokemon use Tackle. In a larger game, this would be loaded from JSON
    // and assigned to the Pokemon's moveset.
    public static readonly Move Tackle = new Move(
        name: "Tackle",
        basePower: 40,
        lungeDuration: 0.12f,
        lungeDistance: 20f,
        pauseBeforeAttack: 0.5f
    );
}
