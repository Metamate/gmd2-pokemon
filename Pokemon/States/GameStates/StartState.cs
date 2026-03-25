using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon;
using Pokemon.Definitions;
using Pokemon.Input;
using Pokemon.Mons;
using GMDCore.States;
using GMDCore;
using GMDCore.Graphics;

namespace Pokemon.States.GameStates;

// Title screen. A random Pokemon sprite slides across the screen every 3 seconds.
// Press Enter/Space to start the game.
public sealed class StartState : GameStateBase
{
    private Texture2D _currentSprite;
    private float _spriteX;
    private float _spriteY;

    private static readonly Color BgColor    = new(188, 188, 188);
    private static readonly Color TitleColor = new(24, 24, 24);
    private static readonly Color ShadowColor = new(45, 184, 45, 124);

    private readonly Texture2D _shadowTex;

    public StartState(Core game) : base(game)
    {
        _shadowTex = TextureFactory.CreateEllipse(Game1.Current.GraphicsDevice, 72, 24, ShadowColor);
    }

    public override void Enter()
    {
        Locator.Audio.PlayIntroMusic();
        PickRandomSprite();
        _spriteX = GameSettings.VirtualWidth / 2f - 32f;
        _spriteY = GameSettings.VirtualHeight / 2f - 36f;

        // Cycle to a new random pokemon sprite every 3 seconds
        Locator.Tweens.Every(3f, CycleSprite);
    }

    private void PickRandomSprite()
    {
        var species = PokemonDefinitions.GetRandom();
        _currentSprite = ContentLoader.GetPokemonSprite(species.BattleSpriteFront);
    }

    private void CycleSprite()
    {
        Locator.Tweens.Tween(0.2f)
            .Add(v => _spriteX = v, _spriteX, -64f)
            .Finish(() =>
            {
                PickRandomSprite();
                _spriteX = GameSettings.VirtualWidth;
                _spriteY = GameSettings.VirtualHeight / 2f - 36f;

                Locator.Tweens.Tween(0.2f)
                    .Add(v => _spriteX = v, _spriteX, GameSettings.VirtualWidth / 2f - 32f);
            });
    }

    public override void Exit()
    {
        Locator.Audio.StopMusic();
        Locator.Tweens.Clear();
    }

    public override void Update(GameTime gameTime)
    {
        if (GameController.Confirm)
        {
            Game.StateStack.Push(new FadeState(Game.StateStack, Color.White, GameSettings.FadeDuration, 0f, 1f,
                () =>
                {
                    Game.StateStack.Pop(); // pop StartState
                    Game.StateStack.Push(new PlayState(Game));
                    Game.StateStack.Push(new DialogueState(Game, Game.StateStack,
                        "Welcome to the world of VIAMon! Walk in the tall grass to fight monsters. " +
                        "Press P to heal. Press Enter or Space to dismiss messages."));
                    Game.StateStack.Push(new FadeState(Game.StateStack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
                }));
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        spriteBatch.Draw(Core.Pixel,
            new Rectangle(0, 0, GameSettings.VirtualWidth, GameSettings.VirtualHeight),
            BgColor);

        spriteBatch.Draw(_shadowTex,
            new Vector2(GameSettings.VirtualWidth / 2f - 72, GameSettings.VirtualHeight / 2f + 4),
            Color.White);

        if (_currentSprite != null)
            spriteBatch.Draw(_currentSprite, new Vector2(_spriteX, _spriteY), Color.White);

        var titleSize    = Game1.LargeFont.MeasureString("VIAMon!");
        var subtitleSize = Game1.MediumFont.MeasureString("Press Enter");

        Game1.LargeFont.Draw(spriteBatch, "VIAMon!",
            new Vector2(GameSettings.VirtualWidth / 2f - titleSize.X / 2f,
                        GameSettings.VirtualHeight / 2f - 72f),
            TitleColor);

        Game1.MediumFont.Draw(spriteBatch, "Press Enter",
            new Vector2(GameSettings.VirtualWidth / 2f - subtitleSize.X / 2f,
                        GameSettings.VirtualHeight / 2f + 68f),
            TitleColor);

        spriteBatch.End();
    }
}
