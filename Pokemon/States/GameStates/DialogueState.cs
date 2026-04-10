using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GMDCore;
using Pokemon.GUI;
using GMDCore.States;

namespace Pokemon.States.GameStates;

// Shows a full-width textbox near the top of the screen with a message.
// Pressing Confirm advances through pages; when the last page is dismissed the state pops itself.
public sealed class DialogueState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly Textbox    _textbox;
    private readonly Action     _onClose;

    public DialogueState(StateStack stack, string text, Action onClose = null)
    {
        _stack   = stack;
        _onClose = onClose ?? (() => { });
        _textbox = new Textbox(6, 6, GameSettings.VirtualWidth - 12, 64, text, Locator.Assets.SmallFont);
    }

    public override void Update(GameTime gameTime)
    {
        _textbox.Update();

        if (_textbox.IsClosed)
        {
            _onClose();
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
