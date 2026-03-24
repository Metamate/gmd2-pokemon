using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon.States.GameStates;

/// <summary>
/// Base class for all game states managed by the StateStack.
/// Each state owns its own SpriteBatch.Begin/End calls inside Draw().
/// Equivalent to Lua's BaseState.
/// </summary>
public abstract class GameStateBase
{
    protected readonly Game1 Game;

    protected GameStateBase(Game1 game) => Game = game;

    public virtual void Enter() { }
    public virtual void Exit()  { }
    public virtual void Update(GameTime gameTime) { }
    public abstract void Draw(SpriteBatch spriteBatch);
}
