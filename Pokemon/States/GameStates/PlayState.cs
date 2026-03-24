using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Definitions;
using Pokemon.Entities;
using Pokemon.Input;
using Pokemon.States.PlayerStates;
using Pokemon.World;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

/// <summary>
/// The overworld play state: renders the level and player, handles the heal shortcut.
/// Equivalent to the Lua PlayState.
/// </summary>
public sealed class PlayState : GameStateBase
{
    private Level _level;

    public PlayState(Core game) : base(game) { }

    public override void Enter()
    {
        var player = new Player();

        player.MapX   = GameSettings.PlayerStartMapX;
        player.MapY   = GameSettings.PlayerStartMapY;
        player.Width  = GameSettings.TileSize;
        player.Height = GameSettings.TileSize;
        player.X      = (player.MapX - 1) * GameSettings.TileSize;
        player.Y      = (player.MapY - 1) * GameSettings.TileSize - player.Height / 2f;

        foreach (var (key, anim) in EntityDefinitions.CreateEntityAnimations(Game1.EntityAtlas))
            player.Animations[key] = anim;

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
            var pokemon = _level.Player.Party.Pokemon[0];
            pokemon.CurrentHp = pokemon.Hp;

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
