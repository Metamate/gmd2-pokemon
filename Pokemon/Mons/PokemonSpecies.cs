namespace Pokemon.Mons;

// Blueprint for a Mon species — base stats and IVs shared by all individuals of that species.
public sealed class PokemonSpecies
{
    public string Name              { get; init; }
    public string BattleSpriteFront { get; init; }
    public string BattleSpriteBack  { get; init; }

    // Base stats (before level scaling)
    public int BaseHp      { get; init; }
    public int BaseAttack  { get; init; }
    public int BaseDefense { get; init; }
    public int BaseSpeed   { get; init; }

    // Individual Values (1–5): higher = more likely to gain that stat on level-up
    public int HpIV      { get; init; }
    public int AttackIV  { get; init; }
    public int DefenseIV { get; init; }
    public int SpeedIV   { get; init; }
}
