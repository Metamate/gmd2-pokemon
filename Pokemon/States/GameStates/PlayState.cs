using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Entities;
using Pokemon.Input;
using Pokemon.States.PlayerStates;
using Pokemon.World;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

// The overworld play state: renders the level and player, handles the heal shortcut.
public sealed class PlayState : GameStateBase
{
    private Level _level;

    public PlayState(Core game) : base(game) { }

    public override void Enter()
    {
        var player = new Player(Game1.EntityAtlas);
        _level = new Level(player, Game1.TileAtlas);

        player.ChangeState(new PlayerIdleState(player, _level, Game.StateStack));

        SoundManager.PlayFieldMusic();
    }

    public override void Exit()
    {
        SoundManager.StopMusic();
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Heal)
        {
            SoundManager.PlayHeal();
            _level.Player.Party.Pokemon[0].Heal();

            Game.StateStack.Push(new DialogueState(Game, Game.StateStack,
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
