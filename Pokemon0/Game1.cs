using GMDCore;
using GMDCore.Graphics;
using GMDCore.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon0.Definitions;
using Pokemon0.States.GameStates;

namespace Pokemon0;

public sealed class Game1 : Core
{
    // The whole game is first drawn at the virtual resolution into this texture,
    // then scaled once into DestinationRectangle.
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

        var tileAtlas   = TextureAtlas.FromGrid(Content.Load<Texture2D>("images/tiles"), GameSettings.TileSize, GameSettings.TileSize);
        var entityAtlas = TextureAtlas.FromGrid(Content.Load<Texture2D>("images/entities"), GameSettings.TileSize, GameSettings.TileSize);

        Locator.Provide(new GameAssets(
            tileAtlas,
            entityAtlas));

        _renderTarget = new RenderTarget2D(
            GraphicsDevice,
            GameSettings.VirtualWidth,
            GameSettings.VirtualHeight);

        ContentLoader.LoadContent(Content);

        StateStack.Push(new PlayState(StateStack));
    }

    protected override void UpdateGame(GameTime gameTime)
    {
        // Tweens fire first so state changes from callbacks are visible to StateStack.Update.
        Locator.Tweens.Update(gameTime);
        StateStack.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Draw the whole game at the virtual resolution first.
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.Black);
        StateStack.Draw(SpriteBatch);
        GraphicsDevice.SetRenderTarget(null);

        // Then scale that one image into the centered letterboxed/pillarboxed area.
        GraphicsDevice.Viewport = new Viewport(
            0,
            0,
            GraphicsDevice.PresentationParameters.BackBufferWidth,
            GraphicsDevice.PresentationParameters.BackBufferHeight);
        GraphicsDevice.Clear(Color.Black);
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
        SpriteBatch.Draw(_renderTarget, DestinationRectangle, Color.White);
        SpriteBatch.End();

        base.Draw(gameTime);
    }
}
