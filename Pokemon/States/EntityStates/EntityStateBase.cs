using Microsoft.Xna.Framework;
using Pokemon.Entities;

namespace Pokemon.States.EntityStates;

/// <summary>
/// Base class for all entity states (idle, walk, …).
/// Equivalent to Lua's EntityBaseState.
/// </summary>
public abstract class EntityStateBase
{
    protected readonly Entity Entity;

    protected EntityStateBase(Entity entity) => Entity = entity;

    public virtual void Enter()  { }
    public virtual void Exit()   { }
    public virtual void Update(GameTime gameTime) { }
}
