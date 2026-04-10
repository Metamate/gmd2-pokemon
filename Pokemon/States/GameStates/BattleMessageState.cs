using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.GUI;
using GMDCore.States;

namespace Pokemon.States.GameStates;

// Shows a battle message textbox at the bottom of the screen.
// When canInput is false the textbox stays visible but ignores input (used during animations).
public sealed class BattleMessageState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly Textbox    _textbox;
    private readonly Action     _onClose;
    private readonly bool       _canInput;

    public BattleMessageState(StateStack stack, string message,
                               Action onClose, bool canInput = true)
    {
        _stack    = stack;
        _onClose  = onClose;
        _canInput = canInput;
        var txtPos = Layout.GetPosition(Anchor.BottomLeft, GameSettings.VirtualWidth, 64);
        _textbox  = new Textbox(txtPos.X, txtPos.Y,
                                GameSettings.VirtualWidth, 64,
                                message, Game1.MediumFont);
    }

    public override void Update(GameTime gameTime)
    {
        if (!_canInput) return;

        _textbox.Update();

        if (_textbox.IsClosed)
        {
            _stack.Pop();
            _onClose();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _textbox.Draw(spriteBatch);
        spriteBatch.End();
    }
}
