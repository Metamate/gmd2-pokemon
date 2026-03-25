using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.GUI;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

// Shows a battle message textbox at the bottom of the screen.
// When canInput is false the textbox stays visible but ignores input (used during animations).
public sealed class BattleMessageState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly Textbox    _textbox;
    private readonly Action     _onClose;
    private readonly bool       _canInput;

    public BattleMessageState(Core game, StateStack stack, string message,
                               Action onClose = null, bool canInput = true)
        : base(game)
    {
        _stack    = stack;
        _onClose  = onClose ?? (() => { });
        _canInput = canInput;
        _textbox  = new Textbox(0, GameSettings.VirtualHeight - 64,
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
