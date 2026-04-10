using Microsoft.Xna.Framework;
using Pokemon;
using Pokemon.Entities;
using Pokemon.Input;
using Pokemon.States.EntityStates;
using Pokemon.States.GameStates;
using Pokemon.World;
using GMDCore.States;

namespace Pokemon.States.PlayerStates;

// Moves the player one tile, then checks whether to continue walking.
// Before committing the move, the destination tile is checked for tall grass.
// Entering a tall-grass tile has a 1-in-10 chance of a random encounter.
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

    // Returns true 1-in-EncounterChance times.
    private static bool RollEncounter()
        => System.Random.Shared.Next(GameSettings.EncounterChance) == 0;

    protected override bool BeforeMove(int toX, int toY)
    {
        int tileId = Level.GrassLayer.GetTile(toX, toY);
        if (tileId != GameSettings.TileTallGrass) return true;
        if (!RollEncounter()) return true;

        // Freeze player in place (PlayerIdleState so input is re-enabled when battle ends)
        Entity.ChangeState(new PlayerIdleState(_player, Level, _stateStack));
        Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));

        Locator.Audio.PauseFieldMusic();
        Locator.Audio.PlayBattleMusic();

        _stateStack.Push(new FadeState(_stateStack, Color.White, GameSettings.FadeDuration, 0f, 1f,
            () =>
            {
                _stateStack.Push(new BattleState(_player, _stateStack));
                _stateStack.Push(new FadeState(_stateStack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
            }));

        return false;
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
