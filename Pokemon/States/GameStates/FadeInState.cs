using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GMDCore.Tweening;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

/// <summary>
/// Fades in a solid color overlay (transparent → opaque), then fires a callback and pops itself.
/// Equivalent to the Lua FadeInState.
/// </summary>
public sealed class FadeInState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly Color      _color;
    private float               _opacity;

    public FadeInState(StateStack stack, Color color, float duration, Action onComplete)
        : base(Game1.Current)
    {
        _stack   = stack;
        _color   = color;
        _opacity = 0f;

        TweenManager.Instance.Tween(duration)
            .Add(v => _opacity = v, 0f, 1f)
            .Finish(() =>
            {
                _stack.Pop();
                onComplete();
            });
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(Game1.Pixel,
            new Rectangle(0, 0, GameSettings.VirtualWidth, GameSettings.VirtualHeight),
            _color * _opacity);
        spriteBatch.End();
    }
}
