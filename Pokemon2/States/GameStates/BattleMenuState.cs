using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GMDCore;
using Pokemon2.GUI;
using GMDCore.States;

namespace Pokemon2.States.GameStates;

// Renders the battle menu. For now, all you can do is run.
public sealed class BattleMenuState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly BattleState _battleState;
    private readonly Menu _menu;

    public BattleMenuState(StateStack stack, BattleState battleState)
    {
        _stack = stack;
        _battleState = battleState;

        var menuPos = Layout.GetPosition(Anchor.BottomRight, 64, 64);
        _menu = new Menu(
            menuPos.X, menuPos.Y, 64, 64,
            new List<Selection.MenuItem>
            {
                new("Run",   OnRunSelected)
            },
            Locator.Assets.MediumFont,
            Locator.Assets.CursorTex);
    }

    private void OnRunSelected()
    {
        _stack.Pop(); // pop this menu

        // Keep the message visible while the delayed fade-out runs.
        _stack.Push(new BattleMessageState(_stack, "You fled successfully!", () => { }, false));

        Locator.Tweens.After(0.5f, () =>
        {
            _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 0f, 1f, () =>
            {
                _stack.Pop(); // pop BattleMessageState
                _stack.Pop(); // pop BattleState (Exit() stops music)
                _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
            }));
        });
    }

    public override void Update(GameTime gameTime) => _menu.Update();

    public override void Draw(SpriteBatch spriteBatch)
    {
        Core.BeginDraw(spriteBatch);
        _menu.Draw(spriteBatch);
        spriteBatch.End();
    }
}
