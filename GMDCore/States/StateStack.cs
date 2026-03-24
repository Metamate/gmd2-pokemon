using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.States;

/// <summary>
/// A stack of game states where only the top state receives Update() calls
/// but all states are rendered from bottom to top.
///
/// States pushed onto the stack have Enter() called; popped states have Exit() called.
/// This allows overlapping states such as a battle menu on top of the battle screen
/// on top of the overworld.
/// </summary>
public sealed class StateStack
{
    private readonly List<GameStateBase> _states = new();

    /// <summary>Push a state onto the top of the stack and call its Enter().</summary>
    public void Push(GameStateBase state)
    {
        _states.Add(state);
        state.Enter();
    }

    /// <summary>Pop the top state and call its Exit().</summary>
    public void Pop()
    {
        if (_states.Count == 0) return;
        var top = _states[^1];
        _states.RemoveAt(_states.Count - 1);
        top.Exit();
    }

    /// <summary>Clear all states without calling Exit on any of them.</summary>
    public void Clear() => _states.Clear();

    /// <summary>Update only the top-most state.</summary>
    public void Update(GameTime gameTime)
    {
        if (_states.Count > 0)
            _states[^1].Update(gameTime);
    }

    /// <summary>Render all states from bottom to top.</summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        foreach (var state in _states)
            state.Draw(spriteBatch);
    }
}
