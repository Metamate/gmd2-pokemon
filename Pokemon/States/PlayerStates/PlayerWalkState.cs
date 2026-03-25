using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Entities;
using Pokemon.Input;
using Pokemon.States.EntityStates;
using Pokemon.States.GameStates;
using Pokemon.World;
using GMDCore.States;

namespace Pokemon.States.PlayerStates;

// Moves the player one tile, then checks whether to continue walking.
// On entering a tall-grass tile there is a 1-in-10 chance of a random encounter.
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

    public override void Enter()
    {
        if (CheckForEncounter()) return;
        AttemptMove();
    }

    private bool CheckForEncounter()
    {
        var tile = Level.GrassLayer.GetTile(_player.MapX, _player.MapY);
        if (tile.Id != GameSettings.TileTallGrass) return false;
        if (System.Random.Shared.Next(GameSettings.EncounterChance) != 0) return false;

        // Freeze player in place (PlayerIdleState so input is re-enabled when battle ends)
        Entity.ChangeState(new PlayerIdleState(_player, Level, _stateStack));
        Entity.ChangeAnimation(AnimationKeys.Idle(Entity.Direction));

        SoundManager.PauseFieldMusic();
        SoundManager.PlayBattleMusic();

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
        // Continue walking if a direction key is still held
        Direction? dir = null;
        if      (GameController.Left)  dir = Direction.Left;
        else if (GameController.Right) dir = Direction.Right;
        else if (GameController.Up)    dir = Direction.Up;
        else if (GameController.Down)  dir = Direction.Down;

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
