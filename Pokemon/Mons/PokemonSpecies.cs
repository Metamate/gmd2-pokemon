namespace Pokemon.Mons;

// Blueprint for a Mon species — base stats and IVs shared by all individuals of that species.
public sealed record PokemonSpecies(
    string Name,
    string BattleSpriteFront,
    string BattleSpriteBack,
    // Base stats (before level scaling)
    int BaseHp,
    int BaseAttack,
    int BaseDefense,
    int BaseSpeed,
    // Individual Values (1–5): higher = more likely to gain that stat on level-up
    int HpIV,
    int AttackIV,
    int DefenseIV,
    int SpeedIV);
