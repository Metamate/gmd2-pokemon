using Microsoft.Xna.Framework;
using Pokemon1.Entities;

namespace Pokemon1.States.EntityStates;

// The entity stands still, playing the appropriate idle animation for its facing direction.
public class EntityIdleState : EntityStateBase
{
    public EntityIdleState(Entity entity) : base(entity) { }

    public override void Enter()
    {
        Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));
    }
}
