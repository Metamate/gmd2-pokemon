using Microsoft.Xna.Framework;
using Pokemon.Entities;

namespace Pokemon.States.EntityStates;

// Base class for entity states (idle, walk, …). Separate from GameStateBase because
// entity states manage per-entity behavior, not game-level flow (menus, battles).
public abstract class EntityStateBase
{
    protected readonly Entity Entity;

    protected EntityStateBase(Entity entity) => Entity = entity;

    public virtual void Enter()  { }
    public virtual void Exit()   { }
    public virtual void Update(GameTime gameTime) { }
}
