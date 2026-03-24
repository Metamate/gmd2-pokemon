using System;
using System.Collections.Generic;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.States.EntityStates;

namespace Pokemon.Entities;

/// <summary>
/// Base entity class for the overworld. Stores grid position, pixel position, direction,
/// animations, and the current entity state (idle/walk).
/// Equivalent to the Lua Entity class.
/// </summary>
public class Entity
{
    // Grid coordinates (1-indexed, matching Lua)
    public int MapX { get; set; }
    public int MapY { get; set; }

    // Pixel position for smooth tweening between tiles
    public float X { get; set; }
    public float Y { get; set; }

    public int Width  { get; set; }
    public int Height { get; set; }

    public Direction Direction { get; set; } = Direction.Down;

    // Named animations keyed by "walk-down", "idle-left", etc.
    public Dictionary<string, Animation> Animations { get; } = new();

    // The animation currently playing
    public string CurrentAnimationKey { get; private set; } = "idle-down";

    private readonly AnimatedSprite _sprite = new();
    public EntityStateBase State { get; private set; }

    public Entity() { }

    /// <summary>Change to a new entity state, calling Exit on the old and Enter on the new.</summary>
    public void ChangeState(EntityStateBase newState)
    {
        State?.Exit();
        State = newState;
        State.Enter();
    }

    /// <summary>Switch the active animation by key name.</summary>
    public void ChangeAnimation(string key)
    {
        if (!Animations.TryGetValue(key, out var anim)) return;
        CurrentAnimationKey = key;
        _sprite.Play(anim);
    }

    public void Update(GameTime gameTime)
    {
        _sprite.Update(gameTime);
        State?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _sprite.Draw(spriteBatch, new Vector2((float)Math.Floor(X), (float)Math.Floor(Y)));
    }
}
