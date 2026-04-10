using GMDCore;
using GMDCore.Graphics;
using GMDCore.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Definitions;
using Pokemon.Mons;
using Pokemon.States.GameStates;

namespace Pokemon;

public sealed class Game1 : Core
{
    private RenderTarget2D _renderTarget;

    public Game1()
        : base("VIAMon",
               GameSettings.WindowWidth,  GameSettings.WindowHeight,
               GameSettings.VirtualWidth, GameSettings.VirtualHeight)
    { }

    protected override void Initialize()
    {
        StateStack = new StateStack();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        var smallFont  = BitmapFont.CreateSmall(Content.Load<Texture2D>("fonts/small_atlas"));
        var mediumFont = BitmapFont.CreateMedium(Content.Load<Texture2D>("fonts/medium_atlas"));
        var largeFont  = BitmapFont.CreateLarge(Content.Load<Texture2D>("fonts/large_atlas"));

        var tileAtlas   = TextureAtlas.FromGrid(Content.Load<Texture2D>("images/tiles"), GameSettings.TileSize, GameSettings.TileSize);
        var entityAtlas = TextureAtlas.FromGrid(Content.Load<Texture2D>("images/entities"), GameSettings.TileSize, GameSettings.TileSize);
        var cursorTex   = Content.Load<Texture2D>("images/cursor");
        var shadowTex   = TextureFactory.CreateEllipse(GraphicsDevice, 72, 24, new Color(45, 184, 45, 124));

        Locator.Provide(new GameAssets(
            smallFont,
            mediumFont,
            largeFont,
            tileAtlas,
            entityAtlas,
            cursorTex,
            shadowTex));

        _renderTarget = new RenderTarget2D(GraphicsDevice, GameSettings.VirtualWidth, GameSettings.VirtualHeight);

        // Species data must load before ContentLoader (sprite paths are derived from it)
        PokemonDefinitions.LoadContent(Content);
        ContentLoader.LoadContent(Content);

        var audio = new SoundManager();
        audio.LoadContent(Content);
        Locator.Provide(audio);

        StateStack.Push(new StartState(StateStack));
    }

    protected override void UpdateGame(GameTime gameTime)
    {
        // Tweens fire first so state changes from callbacks are visible to StateStack.Update.
        Locator.Tweens.Update(gameTime);
        StateStack.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Capture the letterbox destination rect before switching render target
        // (Core.UpdateScreenScaleMatrix sets GraphicsDevice.Viewport to the centered rect)
        var dest = new Rectangle(GraphicsDevice.Viewport.X, GraphicsDevice.Viewport.Y,
                                 GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

        // Draw all states into the virtual-resolution render target (no scaling matrix needed)
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);
        StateStack.Draw(SpriteBatch);

        // Upscale the render target to the screen with point sampling (pixel-perfect)
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Viewport = new Viewport(0, 0,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);
        GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        SpriteBatch.Draw(_renderTarget, dest, Color.White);
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
