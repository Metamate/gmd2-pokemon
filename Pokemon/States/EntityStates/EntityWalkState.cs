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

        Point destination = GetDestination();

        if (!IsInsideMap(destination))
        {
            Entity.ChangeState(new EntityIdleState(Entity));
            Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));
            return;
        }

        if (!BeforeMove(destination))
            return;

        // Grid position updates immediately — tile-based logic (encounters, collision)
        // always uses the destination tile, while X/Y tween smoothly for visuals.
        Entity.MapX = destination.X;
        Entity.MapY = destination.Y;

        float targetX = destination.X * GameSettings.TileSize;
        // Subtract half the entity height so the sprite stands on the tile, not above it
        float targetY = destination.Y * GameSettings.TileSize - Entity.Height / 2f;

        Locator.Tweens.Tween(GameSettings.WalkTweenDuration)
            .Add(v => Entity.X = v, Entity.X, targetX)
            .Add(v => Entity.Y = v, Entity.Y, targetY)
            .Finish(OnMovementComplete);
    }

    // Compute the tile directly in front of the entity based on its facing direction.
    protected Point GetDestination()
    {
        var step = Entity.Direction.ToVector2();
        return new Point(Entity.MapX + (int)step.X, Entity.MapY + (int)step.Y);
    }

    protected bool IsInsideMap(Point tile)
        => tile.X >= 0 && tile.X < GameSettings.MapCols
        && tile.Y >= 0 && tile.Y < GameSettings.MapRows;

    // Hook for subclasses that need to react to the destination tile before the
    // logical move is committed.
    protected virtual bool BeforeMove(Point destination) => true;

    protected virtual void OnMovementComplete()
    {
        Entity.ChangeState(new EntityIdleState(Entity));
    }
}
