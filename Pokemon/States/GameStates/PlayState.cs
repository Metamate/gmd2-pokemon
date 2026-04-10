using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Entities;
using Pokemon.Input;
using Pokemon.States.PlayerStates;
using Pokemon.World;
using GMDCore.States;

namespace Pokemon.States.GameStates;

// The overworld play state: renders the level and player, handles the heal shortcut.
public sealed class PlayState : GameStateBase
{
    private readonly StateStack _stack;
    private Level _level;

    public PlayState(StateStack stack)
    {
        _stack = stack;
    }

    public override void Enter()
    {
        var player = new Player(Game1.EntityAtlas);
        _level = new Level(player, Game1.TileAtlas);

        player.ChangeState(new PlayerIdleState(player, _level, _stack));

        Locator.Audio.PlayFieldMusic();
    }

    public override void Exit()
    {
        Locator.Audio.StopMusic();
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Heal)
        {
            Locator.Audio.PlayHeal();
            _level.Player.Party.Current.Heal();

            _stack.Push(new DialogueState(_stack,
                "Your Pokemon has been healed!"));
        }

        _level.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _level.Draw(spriteBatch);
        spriteBatch.End();
    }
}
