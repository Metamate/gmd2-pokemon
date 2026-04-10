using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Battle;
using GMDCore.GUI;
using Pokemon.Mons;
using GMDCore.States;

namespace Pokemon.States.GameStates;

// Executes one full battle round: the faster Pokemon attacks first, then the slower one.
// After each hit the relevant health bar is tweened. Handles faint and victory outcomes.
public sealed class TakeTurnState : GameStateBase
{
    private readonly StateStack  _stack;
    private readonly BattleState _battle;

    private readonly Mon _firstPokemon;
    private readonly Mon _secondPokemon;
    private readonly BattleSprite    _firstSprite;
    private readonly BattleSprite    _secondSprite;
    private readonly ProgressBar     _firstBar;
    private readonly ProgressBar     _secondBar;

    // Stored between OnVictoryMessageClosed and OnExpTweenComplete.
    private int _earnedExp;

    public TakeTurnState(StateStack stack, BattleState battle)
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
        ExecuteAttack(_firstPokemon, _secondPokemon, Move.Tackle, _firstSprite, _secondSprite,
                      _secondBar, OnFirstAttackComplete);

    // ---- Attack sequence ----

    private void OnFirstAttackComplete()
    {
        _stack.Pop(); // pop first attack message
        if (CheckDeaths()) { _stack.Pop(); return; }

        ExecuteAttack(_secondPokemon, _firstPokemon, Move.Tackle, _secondSprite, _firstSprite,
                      _firstBar, OnSecondAttackComplete);
    }

    private void OnSecondAttackComplete()
    {
        _stack.Pop(); // pop second attack message
        if (CheckDeaths()) { _stack.Pop(); return; }

        _stack.Pop(); // pop TakeTurnState itself
        _stack.Push(new BattleMenuState(_stack, _battle));
    }

    // Plays the full attack animation sequence for one Pokemon attacking another.
    // Each step is nested inside the previous one's completion callback so they
    // play one after another — this is intentional: tweens don't block, so the
    // only way to say "do this, then that" is to start the next thing when the
    // current one finishes.
    private void ExecuteAttack(Mon attacker, Mon defender, Move move,
                                BattleSprite attackerSprite, BattleSprite defenderSprite,
                                ProgressBar defenderBar,
                                Action onEnd)
    {
        // Show the attack message (canInput: false keeps it up during the animation)
        _stack.Push(new BattleMessageState(_stack,
            $"{attacker.Name} used {move.Name}!", () => { }, canInput: false));

        // Step 1: pause briefly, then lunge toward the opponent
        Locator.Tweens.After(move.PauseBeforeAttack, () =>
        {
            Locator.Audio.PlayPowerup();

            float originX = attackerSprite.X;
            float nudge   = attackerSprite == _battle.PlayerSprite
                            ? move.LungeDistance : -move.LungeDistance;

            Locator.Tweens.Tween(move.LungeDuration)
                .Add(v => attackerSprite.X = v, originX, originX + nudge)
                .Finish(() =>
                {
                    // Step 2: spring back to the original position
                    Locator.Audio.PlayHit();

                    Locator.Tweens.Tween(move.LungeDuration)
                        .Add(v => attackerSprite.X = v, originX + nudge, originX)
                        .Finish(() =>
                        {
                            // Step 3: blink the defender to show they were hit
                            Locator.Tweens.Every(GameSettings.AttackBlinkInterval,
                                () => defenderSprite.Blinking = !defenderSprite.Blinking)
                            .Limit(GameSettings.AttackBlinkCount)
                            .Finish(() =>
                            {
                                // Step 4: apply damage and animate the health bar dropping
                                defenderSprite.Blinking = false;
                                int dmg = attacker.CalcDamageTo(defender, move);
                                float targetHp = Math.Max(0, defender.CurrentHp - dmg);

                                Locator.Tweens.Tween(GameSettings.HpTweenDuration)
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
        Locator.Tweens.Tween(GameSettings.FaintTweenDuration)
            .Add(v => _battle.PlayerSprite.Y = v, _battle.PlayerSprite.Y, GameSettings.VirtualHeight)
            .Finish(OnPlayerFainted);
    }

    private void OnPlayerFainted() =>
        _stack.Push(new BattleMessageState(_stack, "You fainted!", OnFaintMessageClosed));

    private void OnFaintMessageClosed() =>
        _stack.Push(new FadeState(_stack, Color.Black, GameSettings.FadeDuration, 0f, 1f, OnFadeToBlackComplete));

    private void OnFadeToBlackComplete()
    {
        _battle.PlayerPokemon.CurrentHp = _battle.PlayerPokemon.Hp;
        _stack.Pop(); // pop BattleState (Exit() stops music)
        Locator.Audio.PlayFieldMusic();
        _stack.Push(new FadeState(_stack, Color.Black, GameSettings.FadeDuration, 1f, 0f, OnFadeFromBlackComplete));
    }

    private void OnFadeFromBlackComplete() =>
        _stack.Push(new DialogueState(_stack, "Your Pokemon has been fully restored; try again!"));

    // ---- Victory path ----

    private void HandleVictory()
    {
        Locator.Tweens.Tween(GameSettings.FaintTweenDuration)
            .Add(v => _battle.OpponentSprite.Y = v, _battle.OpponentSprite.Y, GameSettings.VirtualHeight)
            .Finish(OnOpponentFainted);
    }

    private void OnOpponentFainted()
    {
        Locator.Audio.StopMusic();
        Locator.Audio.PlayVictoryMusic();
        _stack.Push(new BattleMessageState(_stack, "Victory!", OnVictoryMessageClosed));
    }

    private void OnVictoryMessageClosed()
    {
        _earnedExp = _battle.OpponentPokemon.ExpReward;
        _stack.Push(new BattleMessageState(_stack,
            $"You earned {_earnedExp} experience points!", () => { }, canInput: false));
        Locator.Tweens.After(GameSettings.ExpTweenDelay, StartExpTween);
    }

    private void StartExpTween()
    {
        Locator.Audio.PlayExp();
        float targetExp = Math.Min(
            _battle.PlayerPokemon.CurrentExp + _earnedExp,
            _battle.PlayerPokemon.ExpToLevel);

        Locator.Tweens.Tween(GameSettings.ExpTweenDuration)
            .Add(v => _battle.PlayerExpBar.Value = v, _battle.PlayerExpBar.Value, targetExp)
            .Finish(OnExpTweenComplete);
    }

    private void OnExpTweenComplete()
    {
        _stack.Pop(); // pop exp message
        _battle.PlayerPokemon.CurrentExp += _earnedExp;

        if (_battle.PlayerPokemon.CurrentExp >= _battle.PlayerPokemon.ExpToLevel)
        {
            Locator.Audio.PlayLevelup();
            _battle.PlayerPokemon.CurrentExp -= _battle.PlayerPokemon.ExpToLevel;
            var (hp, atk, def, spd) = _battle.PlayerPokemon.LevelUp();
            _stack.Push(new BattleMessageState(_stack,
                $"Level Up! HP+{hp} ATK+{atk} DEF+{def} SPD+{spd}",
                () => FadeToField()));
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
        _stack.Pop(); // pop BattleState (Exit() stops music)
        Locator.Audio.PlayFieldMusic();
        _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
    }

    // TakeTurnState has no visuals of its own; BattleState renders beneath it.
    public override void Draw(SpriteBatch spriteBatch) { }
}
