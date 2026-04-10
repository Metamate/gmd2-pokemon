using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.GUI;
using GMDCore.States;

namespace Pokemon.States.GameStates;

// Renders the Fight / Run menu during a battle.
public sealed class BattleMenuState : GameStateBase
{
    private readonly StateStack  _stack;
    private readonly BattleState _battleState;
    private readonly Menu        _menu;

    public BattleMenuState(StateStack stack, BattleState battleState)
    {
        _stack       = stack;
        _battleState = battleState;

        var menuPos = Layout.GetPosition(Anchor.BottomRight, 64, 64);
        _menu = new Menu(
            menuPos.X, menuPos.Y, 64, 64,
            new List<Selection.MenuItem>
            {
                new("Fight", OnFightSelected),
                new("Run",   OnRunSelected)
            },
            Game1.SmallFont,
            Game1.CursorTex);
    }

    private void OnFightSelected()
    {
        _stack.Pop();
        _stack.Push(new TakeTurnState(_stack, _battleState));
    }

    private void OnRunSelected()
    {
        Locator.Audio.PlayRun();
        _stack.Pop(); // pop this menu

        _stack.Push(new BattleMessageState(_stack, "You fled successfully!", () => { }, false));

        Locator.Tweens.After(0.5f, () =>
        {
            _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 0f, 1f, () =>
            {
                _stack.Pop(); // pop BattleMessageState
                _stack.Pop(); // pop BattleState (Exit() stops music)
                Locator.Audio.PlayFieldMusic();
                _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
            }));
        });
    }

    public override void Update(GameTime gameTime) => _menu.Update();

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _menu.Draw(spriteBatch);
        spriteBatch.End();
    }
}
