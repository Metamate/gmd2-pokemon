using Microsoft.Xna.Framework;
using Pokemon0;
using Pokemon0.Entities;
using Pokemon0.Input;
using Pokemon0.States.EntityStates;
using Pokemon0.World;
using GMDCore.States;

namespace Pokemon0.States.PlayerStates;

// Moves the player one tile, then checks whether to continue walking.
public sealed class PlayerWalkState : EntityWalkState
{
    private readonly Player     _player;
    private readonly StateStack _stateStack;

    public PlayerWalkState(Player player, Level level, StateStack stateStack)
        : base(player, level)
    {
        _player     = player;
        _stateStack = stateStack;
    }

    protected override void OnMovementComplete()
    {
        // Continue walking if a direction key is still held
        Direction? dir = GameController.MovementDirection;

        if (dir.HasValue)
        {
            Entity.Direction = dir.Value;
            Entity.ChangeState(new PlayerWalkState(_player, Level, _stateStack));
        }
        else
        {
            Entity.ChangeState(new PlayerIdleState(_player, Level, _stateStack));
        }
    }
}
