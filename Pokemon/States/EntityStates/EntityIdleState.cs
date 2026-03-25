using Microsoft.Xna.Framework;
using Pokemon.Entities;

namespace Pokemon.States.EntityStates;

/// <summary>
/// The entity stands still, playing the appropriate idle animation for its facing direction.
/// Equivalent to the Lua EntityIdleState.
/// </summary>
public class EntityIdleState : EntityStateBase
{
    public EntityIdleState(Entity entity) : base(entity) { }

    public override void Enter()
    {
        Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));
    }
}
