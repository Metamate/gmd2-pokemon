using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Battle;
using GMDCore.GUI;
using Pokemon.PokemonGame;
using GMDCore.Tweening;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

/// <summary>
/// Executes one full battle round: the faster Pokemon attacks first, then the slower one.
/// After each hit the relevant health bar is tweened. Handles faint and victory outcomes.
/// Equivalent to the Lua TakeTurnState.
/// </summary>
public sealed class TakeTurnState : GameStateBase
{
    private readonly StateStack  _stack;
    private readonly BattleState _battle;

    private readonly PokemonInstance _firstPokemon;
    private readonly PokemonInstance _secondPokemon;
    private readonly BattleSprite    _firstSprite;
    private readonly BattleSprite    _secondSprite;
    private readonly ProgressBar     _firstBar;
    private readonly ProgressBar     _secondBar;

    public TakeTurnState(Core game, StateStack stack, BattleState battle)
        : base(game)
    {
        _stack  = stack;
        _battle = battle;

        if (battle.PlayerPokemon.Speed >= battle.OpponentPokemon.Speed)
        {
            _firstPokemon  = battle.PlayerPokemon;
            _secondPokemon = battle.OpponentPokemon;
            _firstSprite   = battle.PlayerSprite;
            _secondSprite  = battle.OpponentSprite;
            _firstBar      = battle.PlayerHealthBar;
            _secondBar     = battle.OpponentHealthBar;
        }
        else
        {
            _firstPokemon  = battle.OpponentPokemon;
            _secondPokemon = battle.PlayerPokemon;
            _firstSprite   = battle.OpponentSprite;
            _secondSprite  = battle.PlayerSprite;
            _firstBar      = battle.OpponentHealthBar;
            _secondBar     = battle.PlayerHealthBar;
        }
    }

    public override void Enter()
    {
        ExecuteAttack(_firstPokemon, _secondPokemon, _firstSprite, _secondSprite, _firstBar, _secondBar,
            () =>
            {
                _stack.Pop(); // pop first attack message
                if (CheckDeaths()) { _stack.Pop(); return; }

                ExecuteAttack(_secondPokemon, _firstPokemon, _secondSprite, _firstSprite, _secondBar, _firstBar,
                    () =>
                    {
                        _stack.Pop(); // pop second attack message
                        if (CheckDeaths()) { _stack.Pop(); return; }

                        _stack.Pop(); // pop TakeTurnState itself
                        _stack.Push(new BattleMenuState(Game, _stack, _battle));
                    });
            });
    }

    // ---- Attack sequence ----

    private void ExecuteAttack(PokemonInstance attacker, PokemonInstance defender,
                                BattleSprite attackerSprite, BattleSprite defenderSprite,
                                ProgressBar attackerBar, ProgressBar defenderBar,
                                Action onEnd)
    {
        // Non-input message that stays on screen during the animation
        _stack.Push(new BattleMessageState(Game, _stack,
            $"{attacker.Name} attacks {defender.Name}!", () => { }, canInput: false));

        TweenManager.Instance.After(GameSettings.AttackPauseDuration, () =>
        {
            SoundManager.PlayPowerup();

            TweenManager.Instance.Every(GameSettings.AttackBlinkInterval,
                () => attackerSprite.Blinking = !attackerSprite.Blinking)
            .Limit(GameSettings.AttackBlinkCount)
            .Finish(() =>
            {
                attackerSprite.Blinking = false;
                SoundManager.PlayHit();

                TweenManager.Instance.Every(GameSettings.AttackBlinkInterval,
                    () => defenderSprite.Opacity = defenderSprite.Opacity < 0.5f ? 1f : 64f / 255f)
                .Limit(GameSettings.AttackBlinkCount)
                .Finish(() =>
                {
                    defenderSprite.Opacity = 1f;
                    int dmg = Math.Max(1, attacker.Attack - defender.Defense);
                    float targetHp = Math.Max(0, defender.CurrentHp - dmg);

                    TweenManager.Instance.Tween(GameSettings.HpTweenDuration)
                        .Add(v => defenderBar.Value = v, defenderBar.Value, targetHp)
                        .Finish(() =>
                        {
                            defender.CurrentHp = (int)targetHp;
                            onEnd();
                        });
                });
            });
        });
    }

    // ---- Outcome checks ----

    private bool CheckDeaths()
    {
        if (_battle.PlayerPokemon.CurrentHp <= 0)   { HandleFaint();   return true; }
        if (_battle.OpponentPokemon.CurrentHp <= 0) { HandleVictory(); return true; }
        return false;
    }

    private void HandleFaint()
    {
        TweenManager.Instance.Tween(GameSettings.FaintTweenDuration)
            .Add(v => _battle.PlayerSprite.Y = v, _battle.PlayerSprite.Y, GameSettings.VirtualHeight)
            .Finish(() =>
            {
                _stack.Push(new BattleMessageState(Game, _stack, "You fainted!", () =>
                {
                    _stack.Push(new FadeInState(_stack, Color.Black, GameSettings.FadeDuration, () =>
                    {
                        _battle.PlayerPokemon.CurrentHp = _battle.PlayerPokemon.Hp;
                        SoundManager.StopMusic();
                        SoundManager.PlayFieldMusic();

                        _stack.Pop(); // pop BattleState
                        _stack.Push(new FadeOutState(_stack, Color.Black, GameSettings.FadeDuration, () =>
                        {
                            _stack.Push(new DialogueState(Game, _stack,
                                "Your Pokemon has been fully restored; try again!"));
                        }));
                    }));
                }));
            });
    }

    private void HandleVictory()
    {
        TweenManager.Instance.Tween(GameSettings.FaintTweenDuration)
            .Add(v => _battle.OpponentSprite.Y = v, _battle.OpponentSprite.Y, GameSettings.VirtualHeight)
            .Finish(() =>
            {
                SoundManager.StopMusic();
                SoundManager.PlayVictoryMusic();

                _stack.Push(new BattleMessageState(Game, _stack, "Victory!", () =>
                {
                    int exp = _battle.OpponentPokemon.ExpReward;

                    _stack.Push(new BattleMessageState(Game, _stack,
                        $"You earned {exp} experience points!", () => { }, canInput: false));

                    TweenManager.Instance.After(GameSettings.ExpTweenDelay, () =>
                    {
                        SoundManager.PlayExp();

                        float targetExp = Math.Min(
                            _battle.PlayerPokemon.CurrentExp + exp,
                            _battle.PlayerPokemon.ExpToLevel);

                        TweenManager.Instance.Tween(GameSettings.ExpTweenDuration)
                            .Add(v => _battle.PlayerExpBar.Value = v,
                                 _battle.PlayerExpBar.Value, targetExp)
                            .Finish(() =>
                            {
                                _stack.Pop(); // pop exp message

                                _battle.PlayerPokemon.CurrentExp += exp;

                                if (_battle.PlayerPokemon.CurrentExp >= _battle.PlayerPokemon.ExpToLevel)
                                {
                                    SoundManager.PlayLevelup();
                                    _battle.PlayerPokemon.CurrentExp -= _battle.PlayerPokemon.ExpToLevel;
                                    _battle.PlayerPokemon.LevelUp();

                                    _stack.Push(new BattleMessageState(Game, _stack,
                                        "Congratulations! Level Up!",
                                        () => FadeToField()));
                                }
                                else
                                {
                                    FadeToField();
                                }
                            });
                    });
                }));
            });
    }

    private void FadeToField()
    {
        _stack.Push(new FadeInState(_stack, Color.White, GameSettings.FadeDuration, () =>
        {
            SoundManager.StopMusic();
            SoundManager.PlayFieldMusic();
            _stack.Pop(); // pop BattleState
            _stack.Push(new FadeOutState(_stack, Color.White, GameSettings.FadeDuration, () => { }));
        }));
    }

    // TakeTurnState has no visuals of its own; BattleState renders beneath it.
    public override void Draw(SpriteBatch spriteBatch) { }
}
