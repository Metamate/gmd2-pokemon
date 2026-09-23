using Microsoft.Xna.Framework;
using Pokemon4.Entities;
using Pokemon4.Input;
using Pokemon4.States.EntityStates;
using Pokemon4.States.GameStates;
using Pokemon4.World;
using GMDCore.States;

namespace Pokemon4.States.PlayerStates;

// The player stands still. Reads directional input and transitions to a walk state.
public sealed class PlayerIdleState : EntityIdleState
{
    private readonly Level      _level;
    private readonly StateStack _stateStack;

    public PlayerIdleState(Player player, Level level, StateStack stateStack)
        : base(player)
    {
        _level      = level;
        _stateStack = stateStack;
    }

    public override void Update(GameTime gameTime)
    {
        Direction? dir = GameController.MovementDirection;

        if (dir.HasValue)
        {
            Entity.Direction = dir.Value;
            Entity.ChangeState(new PlayerWalkState((Player)Entity, _level, _stateStack));
        }
    }
}
