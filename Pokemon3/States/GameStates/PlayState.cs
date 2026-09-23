using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GMDCore;
using Pokemon3.Entities;
using Pokemon3.Input;
using Pokemon3.States.PlayerStates;
using Pokemon3.World;
using GMDCore.States;

namespace Pokemon3.States.GameStates;

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
        var player = new Player(Locator.Assets.EntityAtlas);
        _level = new Level(player, Locator.Assets.TileAtlas);

        player.ChangeState(new PlayerIdleState(player, _level, _stack));
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Heal)
        {
            _level.Player.Party.Current.Heal();

            _stack.Push(new DialogueState(_stack,
                "Your Pokemon has been healed!"));
        }

        _level.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Core.BeginDraw(spriteBatch);
        _level.Draw(spriteBatch);
        spriteBatch.End();
    }
}
