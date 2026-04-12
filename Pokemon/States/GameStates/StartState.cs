using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon;
using Pokemon.Definitions;
using Pokemon.Input;
using Pokemon.Mons;
using GMDCore.States;
using GMDCore;

namespace Pokemon.States.GameStates;

// Title screen. A random Pokemon sprite slides across the screen every 3 seconds.
// Press Enter/Space to start the game.
public sealed class StartState : GameStateBase
{
    private readonly StateStack _stack;
    private Texture2D _currentSprite;
    private float _spriteX;
    private float _spriteY;

    private static readonly Color BgColor    = new(188, 188, 188);
    private static readonly Color TitleColor = new(24, 24, 24);

    public StartState(StateStack stack)
    {
        _stack = stack;
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
            _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 0f, 1f,
                () =>
                {
                    _stack.Pop(); // pop StartState
                    _stack.Push(new PlayState(_stack));
                    _stack.Push(new DialogueState(_stack,
                        "Welcome to the world of VIAMon! Walk in the tall grass to fight monsters. " +
                        "Press P to heal. Press Enter or Space to dismiss messages."));
                    _stack.Push(new FadeState(_stack, Color.White, GameSettings.FadeDuration, 1f, 0f, () => { }));
                }));
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Core.BeginDraw(spriteBatch);

        spriteBatch.Draw(Core.Pixel,
            new Rectangle(0, 0, GameSettings.VirtualWidth, GameSettings.VirtualHeight),
            BgColor);

        spriteBatch.Draw(Locator.Assets.ShadowTex,
            new Vector2(GameSettings.VirtualWidth / 2f - 72, GameSettings.VirtualHeight / 2f + 4),
            Color.White);

        if (_currentSprite != null)
            spriteBatch.Draw(_currentSprite, new Vector2(_spriteX, _spriteY), Color.White);

        var titleSize    = Locator.Assets.LargeFont.MeasureString("VIAMon!");
        var subtitleSize = Locator.Assets.MediumFont.MeasureString("Press Enter");

        Locator.Assets.LargeFont.Draw(spriteBatch, "VIAMon!",
            new Vector2(GameSettings.VirtualWidth / 2f - titleSize.X / 2f,
                        GameSettings.VirtualHeight / 2f - 72f),
            TitleColor);

        Locator.Assets.MediumFont.Draw(spriteBatch, "Press Enter",
            new Vector2(GameSettings.VirtualWidth / 2f - subtitleSize.X / 2f,
                        GameSettings.VirtualHeight / 2f + 68f),
            TitleColor);

        spriteBatch.End();
    }
}
