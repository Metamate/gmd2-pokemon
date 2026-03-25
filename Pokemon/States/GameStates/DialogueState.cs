using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.GUI;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

// Shows a full-width textbox near the top of the screen with a message.
// Pressing Confirm advances through pages; when the last page is dismissed the state pops itself.
public sealed class DialogueState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly Textbox    _textbox;
    private readonly Action     _onClose;

    public DialogueState(Core game, StateStack stack, string text, Action onClose = null)
        : base(game)
    {
        _stack   = stack;
        _onClose = onClose ?? (() => { });
        _textbox = new Textbox(6, 6, GameSettings.VirtualWidth - 12, 64, text, Game1.SmallFont);
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
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _textbox.Draw(spriteBatch);
        spriteBatch.End();
    }
}
