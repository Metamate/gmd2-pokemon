using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GMDCore;
using Pokemon0.Entities;
using Pokemon0.States.PlayerStates;
using Pokemon0.World;
using GMDCore.States;

namespace Pokemon0.States.GameStates;

// The overworld play state: renders the level and player.
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
        var player = new Player(Locator.Assets.EntityAtlas);
        _level = new Level(player, Locator.Assets.Tileset);

        player.ChangeState(new PlayerIdleState(player, _level, _stack));
    }

    public override void Update(GameTime gameTime)
    {
        _level.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Core.BeginDraw(spriteBatch);
        _level.Draw(spriteBatch);
        spriteBatch.End();
    }
}
