using Microsoft.Xna.Framework;
using Pokemon.Entities;

namespace Pokemon.States.EntityStates;

// Base class for all entity states (idle, walk, …).
public abstract class EntityStateBase
{
    protected readonly Entity Entity;

    protected EntityStateBase(Entity entity) => Entity = entity;

    public virtual void Enter()  { }
    public virtual void Exit()   { }
    public virtual void Update(GameTime gameTime) { }
}
