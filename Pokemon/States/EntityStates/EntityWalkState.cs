using Microsoft.Xna.Framework;
using Pokemon;
using Pokemon.Entities;
using Pokemon.World;

namespace Pokemon.States.EntityStates;

// Moves the entity one tile in its current direction via a smooth position tween,
// then decides whether to continue walking or return to idle.
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

        if (toX < 0 || toX >= GameSettings.MapCols || toY < 0 || toY >= GameSettings.MapRows)
        {
            Entity.ChangeState(new EntityIdleState(Entity));
            Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));
            return;
        }

        // Grid position updates immediately — tile-based logic (encounters, collision)
        // always uses the destination tile, while X/Y tween smoothly for visuals.
        Entity.MapX = toX;
        Entity.MapY = toY;

        float targetX = toX * GameSettings.TileSize;
        // Subtract half the entity height so the sprite stands on the tile, not above it
        float targetY = toY * GameSettings.TileSize - Entity.Height / 2f;

        Locator.Tweens.Tween(GameSettings.WalkTweenDuration)
            .Add(v => Entity.X = v, Entity.X, targetX)
            .Add(v => Entity.Y = v, Entity.Y, targetY)
            .Finish(OnMovementComplete);
    }

    protected virtual void OnMovementComplete()
    {
        Entity.ChangeState(new EntityIdleState(Entity));
    }
}
