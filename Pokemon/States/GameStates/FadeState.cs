using System;
using GMDCore;
using GMDCore.States;
using GMDCore.Tweening;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon.States.GameStates;

// Renders a solid color overlay whose opacity is tweened from
// fromOpacity to toOpacity.
// Pops itself and fires onComplete when done.
// 
// Use fromOpacity=0, toOpacity=1 to fade in; reverse to fade out.
public sealed class FadeState : GameStateBase
{
    private readonly StateStack _stack;
    private readonly Color      _color;
    private float               _opacity;

    public FadeState(StateStack stack, Color color, float duration,
                     float fromOpacity, float toOpacity, Action onComplete)
        : base(Game1.Current)
    {
        _stack   = stack;
        _color   = color;
        _opacity = fromOpacity;

        TweenManager.Instance.Tween(duration)
            .Add(v => _opacity = v, fromOpacity, toOpacity)
            .Finish(() =>
            {
                _stack.Pop();
                onComplete();
            });
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(Core.Pixel,
            new Rectangle(0, 0, GameSettings.VirtualWidth, GameSettings.VirtualHeight),
            _color * _opacity);
        spriteBatch.End();
    }
}
