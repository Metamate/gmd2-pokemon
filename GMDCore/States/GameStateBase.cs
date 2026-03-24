using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.States;

/// <summary>
/// Base class for all game states managed by the StateStack.
/// Each state owns its own SpriteBatch.Begin/End calls inside Draw().
/// </summary>
public abstract class GameStateBase
{
    protected readonly Core Game;

    protected GameStateBase(Core game) => Game = game;

    public virtual void Enter() { }
    public virtual void Exit()  { }
    public virtual void Update(GameTime gameTime) { }
    public abstract void Draw(SpriteBatch spriteBatch);
}
