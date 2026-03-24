using Microsoft.Xna.Framework;
using Pokemon.Entities;
using GMDCore.Tweening;
using Pokemon.World;

namespace Pokemon.States.EntityStates;

/// <summary>
/// Moves the entity one tile in its current direction via a smooth position tween,
/// then decides whether to continue walking or return to idle.
/// Equivalent to the Lua EntityWalkState.
/// </summary>
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
        Entity.ChangeAnimation($"walk-{Entity.Direction.ToKey()}");

        int toX = Entity.MapX;
        int toY = Entity.MapY;

        switch (Entity.Direction)
        {
            case Direction.Left:  toX--; break;
            case Direction.Right: toX++; break;
            case Direction.Up:    toY--; break;
            case Direction.Down:  toY++; break;
        }

        // Enforce map bounds (matching Lua EntityWalkState constraint)
        if (toX < 1 || toX > GameSettings.MapCols || toY < 1 || toY > GameSettings.MapRows)
        {
            Entity.ChangeState(new EntityIdleState(Entity));
            Entity.ChangeAnimation($"idle-{Entity.Direction.ToKey()}");
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
        // Default: transition back to idle
        Entity.ChangeState(new EntityIdleState(Entity));
    }
}
