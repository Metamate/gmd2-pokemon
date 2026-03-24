using System;

namespace Pokemon.PokemonGame;

/// <summary>
/// A runtime Pokemon instance with stats calculated from its species definition and level.
/// Equivalent to the Lua Pokemon class.
/// </summary>
public sealed class PokemonInstance
{
    public string Name              { get; }
    public string BattleSpriteFront { get; }
    public string BattleSpriteBack  { get; }

    // Individual values (determine stat growth probability per level)
    public int HpIV      { get; }
    public int AttackIV  { get; }
    public int DefenseIV { get; }
    public int SpeedIV   { get; }

    // Current stats (grow on level-up)
    public int Hp      { get; private set; }
    public int Attack  { get; private set; }
    public int Defense { get; private set; }
    public int Speed   { get; private set; }

    public int Level       { get; private set; }
    public int CurrentHp   { get; set; }
    public int CurrentExp  { get; set; }
    public int ExpToLevel  { get; private set; }

    public PokemonInstance(PokemonSpecies def, int level)
    {
        Name              = def.Name;
        BattleSpriteFront = def.BattleSpriteFront;
        BattleSpriteBack  = def.BattleSpriteBack;

        HpIV      = def.HpIV;
        AttackIV  = def.AttackIV;
        DefenseIV = def.DefenseIV;
        SpeedIV   = def.SpeedIV;

        Hp      = def.BaseHp;
        Attack  = def.BaseAttack;
        Defense = def.BaseDefense;
        Speed   = def.BaseSpeed;

        Level      = level;
        CurrentExp = 0;
        ExpToLevel = (int)(level * level * 5 * 0.75f);

        // Apply level-up stat growth for each level reached
        for (int i = 0; i < level; i++)
            RollStatsLevelUp();

        CurrentHp = Hp;
    }

    /// <summary>
    /// Rolls stats for one level. Each stat's IV (1–5) is tested 3 times against a d6:
    /// if the roll is ≤ IV the stat increases by 1. Returns the four increases.
    /// </summary>
    public (int hpGain, int atkGain, int defGain, int spdGain) RollStatsLevelUp()
    {
        int hpGain = 0, atkGain = 0, defGain = 0, spdGain = 0;
        var rng = Random.Shared;

        for (int i = 0; i < 3; i++) if (rng.Next(1, 7) <= HpIV)      { Hp++;      hpGain++;  }
        for (int i = 0; i < 3; i++) if (rng.Next(1, 7) <= AttackIV)  { Attack++;  atkGain++; }
        for (int i = 0; i < 3; i++) if (rng.Next(1, 7) <= DefenseIV) { Defense++; defGain++; }
        for (int i = 0; i < 3; i++) if (rng.Next(1, 7) <= SpeedIV)   { Speed++;   spdGain++; }

        return (hpGain, atkGain, defGain, spdGain);
    }

    /// <summary>
    /// Advance one level: increment level, recalculate ExpToLevel, roll stats.
    /// Returns the stat gains for display purposes.
    /// </summary>
    public (int hpGain, int atkGain, int defGain, int spdGain) LevelUp()
    {
        Level++;
        ExpToLevel = (int)(Level * Level * 5 * 0.75f);
        return RollStatsLevelUp();
    }

    /// <summary>Exp reward earned when this Pokemon is defeated.</summary>
    public int ExpReward => (HpIV + AttackIV + DefenseIV + SpeedIV) * Level;
}
