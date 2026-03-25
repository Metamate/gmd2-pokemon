using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon;
using Pokemon.GUI;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

// Renders the Fight / Run menu during a battle.
public sealed class BattleMenuState : GameStateBase
{
    private readonly StateStack  _stack;
    private readonly BattleState _battleState;
    private readonly Menu        _menu;

    public BattleMenuState(Core game, StateStack stack, BattleState battleState)
        : base(game)
    {
        _stack       = stack;
        _battleState = battleState;

        _menu = new Menu(
            GameSettings.VirtualWidth - 64, GameSettings.VirtualHeight - 64, 64, 64,
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
        _stack.Push(new TakeTurnState(Game, _stack, _battleState));
    }

    private void OnRunSelected()
    {
        Locator.Audio.PlayRun();
        _stack.Pop(); // pop this menu

        _stack.Push(new BattleMessageState(Game, _stack, "You fled successfully!", () => { }, false));

        Locator.Tweens.After(0.5f, () =>
        {
            _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 0f, 1f, () =>
            {
                Locator.Audio.PlayFieldMusic();
                _stack.Pop(); // pop BattleMessageState
                _stack.Pop(); // pop BattleState
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
