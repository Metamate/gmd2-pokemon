using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GMDCore;
using Pokemon.GUI;
using GMDCore.States;
using Pokemon.Input;

namespace Pokemon.States.GameStates;

// Shows a full-width textbox near the top of the screen with a message.
// Pressing Confirm advances through pages; when the last page is dismissed the state pops itself.
public sealed class PauseState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly Textbox    _textbox;

    public PauseState(StateStack stack, Action onClose = null)
    {
        _stack   = stack;
        _textbox = new Textbox(6, 6, GameSettings.VirtualWidth - 12, 64, "Game Paused", Locator.Assets.MediumFont);
    }

    public override void Enter()
    {
        Locator.Audio.StopMusic();
    }

        public override void Exit()
    {
        Locator.Audio.PlayFieldMusic();
    }

    public override void Update(GameTime gameTime)
    {
        _textbox.Update();

        if (GameController.Pause)
        {
            _stack.Pop();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Core.BeginDraw(spriteBatch);
        _textbox.Draw(spriteBatch);
        spriteBatch.End();
    }
}
