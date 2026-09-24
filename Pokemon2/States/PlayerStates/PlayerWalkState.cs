using Microsoft.Xna.Framework;
using Pokemon2;
using Pokemon2.Entities;
using Pokemon2.Input;
using Pokemon2.States.EntityStates;
using Pokemon2.States.GameStates;
using Pokemon2.World;
using GMDCore.States;

namespace Pokemon2.States.PlayerStates;

// Moves the player one tile, then checks whether to continue walking.
// After the move visually finishes, landing on a tall-grass tile has a
// 1-in-10 chance of triggering a random encounter.
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

    private bool TryStartEncounter()
    {
        int tileId = Level.GrassLayer.GetTile(Entity.MapX, Entity.MapY).GraphicId;
        if (tileId != GameSettings.TileTallGrass) return false;
        if (!RollEncounter()) return false;

        // Freeze player in place (PlayerIdleState so input is re-enabled when battle ends)
        Entity.ChangeState(new PlayerIdleState(_player, Level, _stateStack));
        Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));

        _stateStack.Push(new FadeState(_stateStack, Color.White, GameSettings.FadeDuration, 0f, 1f,
            () =>
            {
                _stateStack.Push(new BattleState(_player, _stateStack));
                _stateStack.Push(new FadeState(_stateStack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
            }));

        return true;
    }

    protected override void OnMovementComplete()
    {
        if (TryStartEncounter())
            return;

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
