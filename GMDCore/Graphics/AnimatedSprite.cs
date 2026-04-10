using System;
using Microsoft.Xna.Framework;

namespace GMDCore.Graphics;

public class AnimatedSprite : Sprite
{
    private int _currentFrame;
    private TimeSpan _elapsed;
    private Animation _animation;

    public void Play(Animation animation)
    {
        if (_animation == animation) return;

        _animation = animation;
        _currentFrame = 0;
        _elapsed = TimeSpan.Zero;
        Region = _animation.Frames[0];
    }

    public void Update(GameTime gameTime)
    {
        if (_animation == null) return;

        _elapsed += gameTime.ElapsedGameTime;

        if (_elapsed >= _animation.Delay)
        {
            _elapsed -= _animation.Delay;
            _currentFrame++;

            if (_currentFrame >= _animation.Frames.Count)
            {
                if (_animation.Loop)
                {
                    _currentFrame = 0;
                }
                else
                {
                    _currentFrame = _animation.Frames.Count - 1;
                }
            }

            Region = _animation.Frames[_currentFrame];
        }
    }
}
