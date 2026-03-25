using System.Collections.Generic;

namespace Pokemon.PokemonGame;

public sealed class PokemonSpecies
{
    public string Name              { get; init; }
    public string BattleSpriteFront { get; init; }   // Content path key
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

// Registry of all Pokemon species, equivalent to PokemonDefinitions.
public static class PokemonDefinitions
{
    public static readonly IReadOnlyList<PokemonSpecies> All = new List<PokemonSpecies>
    {
        new()
        {
            Name = "Aardart",
            BattleSpriteFront = "images/pokemon/aardart-front",
            BattleSpriteBack  = "images/pokemon/aardart-back",
            BaseHp = 14, BaseAttack = 9, BaseDefense = 5, BaseSpeed = 6,
            HpIV = 3, AttackIV = 4, DefenseIV = 2, SpeedIV = 3
        },
        new()
        {
            Name = "Agnite",
            BattleSpriteFront = "images/pokemon/agnite-front",
            BattleSpriteBack  = "images/pokemon/agnite-back",
            BaseHp = 12, BaseAttack = 7, BaseDefense = 3, BaseSpeed = 7,
            HpIV = 3, AttackIV = 4, DefenseIV = 2, SpeedIV = 4
        },
        new()
        {
            Name = "Anoleaf",
            BattleSpriteFront = "images/pokemon/anoleaf-front",
            BattleSpriteBack  = "images/pokemon/anoleaf-back",
            BaseHp = 11, BaseAttack = 5, BaseDefense = 5, BaseSpeed = 6,
            HpIV = 3, AttackIV = 3, DefenseIV = 3, SpeedIV = 4
        },
        new()
        {
            Name = "Bamboon",
            BattleSpriteFront = "images/pokemon/bamboon-front",
            BattleSpriteBack  = "images/pokemon/bamboon-back",
            BaseHp = 13, BaseAttack = 6, BaseDefense = 4, BaseSpeed = 7,
            HpIV = 3, AttackIV = 3, DefenseIV = 2, SpeedIV = 5
        },
        new()
        {
            Name = "Cardiwing",
            BattleSpriteFront = "images/pokemon/cardiwing-front",
            BattleSpriteBack  = "images/pokemon/cardiwing-back",
            BaseHp = 14, BaseAttack = 7, BaseDefense = 3, BaseSpeed = 7,
            HpIV = 3, AttackIV = 4, DefenseIV = 2, SpeedIV = 4
        }
    };

    // Returns a random species definition.
    public static PokemonSpecies GetRandom()
        => All[System.Random.Shared.Next(All.Count)];
}
