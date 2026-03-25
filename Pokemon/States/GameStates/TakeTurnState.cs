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

// Executes one full battle round: the faster Pokemon attacks first, then the slower one.
// After each hit the relevant health bar is tweened. Handles faint and victory outcomes.
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

    // Stored between OnVictoryMessageClosed and OnExpTweenComplete.
    private int _earnedExp;

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

    public override void Enter() =>
        ExecuteAttack(_firstPokemon, _secondPokemon, _firstSprite, _secondSprite,
                      _firstBar, _secondBar, OnFirstAttackComplete);

    // ---- Attack sequence ----

    private void OnFirstAttackComplete()
    {
        _stack.Pop(); // pop first attack message
        if (CheckDeaths()) { _stack.Pop(); return; }

        ExecuteAttack(_secondPokemon, _firstPokemon, _secondSprite, _firstSprite,
                      _secondBar, _firstBar, OnSecondAttackComplete);
    }

    private void OnSecondAttackComplete()
    {
        _stack.Pop(); // pop second attack message
        if (CheckDeaths()) { _stack.Pop(); return; }

        _stack.Pop(); // pop TakeTurnState itself
        _stack.Push(new BattleMenuState(Game, _stack, _battle));
    }

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

            // Lunge toward the opponent, then spring back
            float originX = attackerSprite.X;
            float nudge   = attackerSprite == _battle.PlayerSprite
                            ? GameSettings.TackleNudge : -GameSettings.TackleNudge;

            TweenManager.Instance.Tween(GameSettings.TackleDuration)
                .Add(v => attackerSprite.X = v, originX, originX + nudge)
                .Finish(() =>
                {
                    SoundManager.PlayHit();

                    TweenManager.Instance.Tween(GameSettings.TackleDuration)
                        .Add(v => attackerSprite.X = v, originX + nudge, originX)
                        .Finish(() =>
                        {
                            // Defender blinks (original Pokemon hide/show style)
                            TweenManager.Instance.Every(GameSettings.AttackBlinkInterval,
                                () => defenderSprite.Blinking = !defenderSprite.Blinking)
                            .Limit(GameSettings.AttackBlinkCount)
                            .Finish(() =>
                            {
                                defenderSprite.Blinking = false;
                                int dmg = attacker.CalcDamageTo(defender);
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
        });
    }

    // ---- Outcome checks ----

    private bool CheckDeaths()
    {
        if (_battle.PlayerPokemon.CurrentHp <= 0)   { HandleFaint();   return true; }
        if (_battle.OpponentPokemon.CurrentHp <= 0) { HandleVictory(); return true; }
        return false;
    }

    // ---- Faint path ----

    private void HandleFaint()
    {
        TweenManager.Instance.Tween(GameSettings.FaintTweenDuration)
            .Add(v => _battle.PlayerSprite.Y = v, _battle.PlayerSprite.Y, GameSettings.VirtualHeight)
            .Finish(OnPlayerFainted);
    }

    private void OnPlayerFainted() =>
        _stack.Push(new BattleMessageState(Game, _stack, "You fainted!", OnFaintMessageClosed));

    private void OnFaintMessageClosed() =>
        _stack.Push(new FadeState(_stack, Color.Black, GameSettings.FadeDuration, 0f, 1f, OnFadeToBlackComplete));

    private void OnFadeToBlackComplete()
    {
        _battle.PlayerPokemon.CurrentHp = _battle.PlayerPokemon.Hp;
        SoundManager.StopMusic();
        SoundManager.PlayFieldMusic();
        _stack.Pop(); // pop BattleState
        _stack.Push(new FadeState(_stack, Color.Black, GameSettings.FadeDuration, 1f, 0f, OnFadeFromBlackComplete));
    }

    private void OnFadeFromBlackComplete() =>
        _stack.Push(new DialogueState(Game, _stack, "Your Pokemon has been fully restored; try again!"));

    // ---- Victory path ----

    private void HandleVictory()
    {
        TweenManager.Instance.Tween(GameSettings.FaintTweenDuration)
            .Add(v => _battle.OpponentSprite.Y = v, _battle.OpponentSprite.Y, GameSettings.VirtualHeight)
            .Finish(OnOpponentFainted);
    }

    private void OnOpponentFainted()
    {
        SoundManager.StopMusic();
        SoundManager.PlayVictoryMusic();
        _stack.Push(new BattleMessageState(Game, _stack, "Victory!", OnVictoryMessageClosed));
    }

    private void OnVictoryMessageClosed()
    {
        _earnedExp = _battle.OpponentPokemon.ExpReward;
        _stack.Push(new BattleMessageState(Game, _stack,
            $"You earned {_earnedExp} experience points!", () => { }, canInput: false));
        TweenManager.Instance.After(GameSettings.ExpTweenDelay, StartExpTween);
    }

    private void StartExpTween()
    {
        SoundManager.PlayExp();
        float targetExp = Math.Min(
            _battle.PlayerPokemon.CurrentExp + _earnedExp,
            _battle.PlayerPokemon.ExpToLevel);

        TweenManager.Instance.Tween(GameSettings.ExpTweenDuration)
            .Add(v => _battle.PlayerExpBar.Value = v, _battle.PlayerExpBar.Value, targetExp)
            .Finish(OnExpTweenComplete);
    }

    private void OnExpTweenComplete()
    {
        _stack.Pop(); // pop exp message
        _battle.PlayerPokemon.CurrentExp += _earnedExp;

        if (_battle.PlayerPokemon.CurrentExp >= _battle.PlayerPokemon.ExpToLevel)
        {
            SoundManager.PlayLevelup();
            _battle.PlayerPokemon.CurrentExp -= _battle.PlayerPokemon.ExpToLevel;
            _battle.PlayerPokemon.LevelUp();
            _stack.Push(new BattleMessageState(Game, _stack, "Congratulations! Level Up!", () => FadeToField()));
        }
        else
        {
            FadeToField();
        }
    }

    private void FadeToField() =>
        _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 0f, 1f, OnFadeToWhiteComplete));

    private void OnFadeToWhiteComplete()
    {
        SoundManager.StopMusic();
        SoundManager.PlayFieldMusic();
        _stack.Pop(); // pop BattleState
        _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
    }

    // TakeTurnState has no visuals of its own; BattleState renders beneath it.
    public override void Draw(SpriteBatch spriteBatch) { }
}
