using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Definitions;
using Pokemon.Input;
using Pokemon.PokemonGame;
using GMDCore.Tweening;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

// Title screen. A random Pokemon sprite slides across the screen every 3 seconds.
// Press Enter/Space to start the game.
// Equivalent to the Lua StartState.
public sealed class StartState : GameStateBase
{
    private Texture2D _currentSprite;
    private float _spriteX;
    private float _spriteY;

    private static readonly Color BgColor    = new(188, 188, 188);
    private static readonly Color TitleColor = new(24, 24, 24);
    private static readonly Color ShadowColor = new(45, 184, 45, 124);

    public StartState(Core game) : base(game) { }

    public override void Enter()
    {
        SoundManager.PlayIntroMusic();
        PickRandomSprite();
        _spriteX = GameSettings.VirtualWidth / 2f - 32f;
        _spriteY = GameSettings.VirtualHeight / 2f - 16f;

        // Cycle to a new random pokemon sprite every 3 seconds
        TweenManager.Instance.Every(3f, CycleSprite);
    }

    private void PickRandomSprite()
    {
        var species = PokemonDefinitions.GetRandom();
        _currentSprite = EntityDefinitions.GetPokemonSprite(species.BattleSpriteFront);
    }

    private void CycleSprite()
    {
        TweenManager.Instance.Tween(0.2f)
            .Add(v => _spriteX = v, _spriteX, -64f)
            .Finish(() =>
            {
                PickRandomSprite();
                _spriteX = GameSettings.VirtualWidth;
                _spriteY = GameSettings.VirtualHeight / 2f - 16f;

                TweenManager.Instance.Tween(0.2f)
                    .Add(v => _spriteX = v, _spriteX, GameSettings.VirtualWidth / 2f - 32f);
            });
    }

    public override void Exit()
    {
        SoundManager.StopMusic();
        TweenManager.Instance.Clear();
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
                        "Welcome to the world of 50Mon! Walk in the tall grass to fight monsters. " +
                        "Press P to heal. Press Enter or Space to dismiss messages."));
                    Game.StateStack.Push(new FadeState(Game.StateStack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
                }));
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        spriteBatch.Draw(Game1.Pixel,
            new Rectangle(0, 0, GameSettings.VirtualWidth, GameSettings.VirtualHeight),
            BgColor);

        spriteBatch.Draw(Game1.Pixel,
            new Rectangle((int)(GameSettings.VirtualWidth / 2 - 72),
                          (int)(GameSettings.VirtualHeight / 2 + 8), 144, 48),
            ShadowColor);

        if (_currentSprite != null)
            spriteBatch.Draw(_currentSprite, new Vector2(_spriteX, _spriteY), Color.White);

        var titleSize    = Game1.LargeFont.MeasureString("50-Mon!");
        var subtitleSize = Game1.SmallFont.MeasureString("Press Enter");

        Game1.LargeFont.Draw(spriteBatch, "50-Mon!",
            new Vector2(GameSettings.VirtualWidth / 2f - titleSize.X / 2f,
                        GameSettings.VirtualHeight / 2f - 72f),
            TitleColor);

        Game1.SmallFont.Draw(spriteBatch, "Press Enter",
            new Vector2(GameSettings.VirtualWidth / 2f - subtitleSize.X / 2f,
                        GameSettings.VirtualHeight / 2f + 68f),
            TitleColor);

        spriteBatch.End();
    }
}
