using Microsoft.Xna.Framework;
using Pokemon.Entities;
using Pokemon.Input;
using Pokemon.States.EntityStates;
using Pokemon.States.GameStates;
using Pokemon.World;
using GMDCore.States;

namespace Pokemon.States.PlayerStates;

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
        Direction? dir = null;

        if      (GameController.Left)  dir = Direction.Left;
        else if (GameController.Right) dir = Direction.Right;
        else if (GameController.Up)    dir = Direction.Up;
        else if (GameController.Down)  dir = Direction.Down;

        if (dir.HasValue)
        {
            Entity.Direction = dir.Value;
            Entity.ChangeState(new PlayerWalkState((Player)Entity, _level, _stateStack));
        }
    }
}
