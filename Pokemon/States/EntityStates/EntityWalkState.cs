using Microsoft.Xna.Framework;
using Pokemon.Entities;
using GMDCore.Tweening;
using Pokemon.World;

namespace Pokemon.States.EntityStates;

// Moves the entity one tile in its current direction via a smooth position tween,
// then decides whether to continue walking or return to idle.
// Equivalent to the Lua EntityWalkState.
public class EntityWalkState : EntityStateBase
{
    protected readonly Level Level;

    public EntityWalkState(Entity entity, Level level) : base(entity)
    {
        Level = level;
    }

    public override void Enter()
    {
        AttemptMove();
    }

    protected virtual void AttemptMove()
    {
        Entity.ChangeAnimation(AnimationKeys.Walk(Entity.Direction));

        var step = Entity.Direction.ToVector2();
        int toX  = Entity.MapX + (int)step.X;
        int toY  = Entity.MapY + (int)step.Y;

        // Enforce map bounds (matching Lua EntityWalkState constraint)
        if (toX < 1 || toX > GameSettings.MapCols || toY < 1 || toY > GameSettings.MapRows)
        {
            Entity.ChangeState(new EntityIdleState(Entity));
            Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));
            return;
        }

        Entity.MapX = toX;
        Entity.MapY = toY;

        float targetX = (toX - 1) * GameSettings.TileSize;
        float targetY = (toY - 1) * GameSettings.TileSize - Entity.Height / 2f;

        TweenManager.Instance.Tween(GameSettings.WalkTweenDuration)
            .Add(v => Entity.X = v, Entity.X, targetX)
            .Add(v => Entity.Y = v, Entity.Y, targetY)
            .Finish(OnMovementComplete);
    }

    protected virtual void OnMovementComplete()
    {
        Entity.ChangeState(new EntityIdleState(Entity));
    }
}
