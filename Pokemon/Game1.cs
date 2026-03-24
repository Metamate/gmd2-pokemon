using GMDCore;
using GMDCore.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Definitions;
using Pokemon.States;
using Pokemon.States.GameStates;
using Pokemon.Tweening;

namespace Pokemon;

public sealed class Game1 : Core
{
    // Static accessors so states created without a Game1 reference can reach shared resources.
    public static Game1 Current { get; private set; }

    public new Matrix ScreenScaleMatrix => base.ScreenScaleMatrix;

    // Pixel-white 1×1 texture used by Panel, ProgressBar, and fade overlays.
    public static Texture2D Pixel { get; private set; }

    // Three font sizes matching the Lua gFonts table (pixel-perfect bitmap fonts, no AA).
    public static GUI.BitmapFont SmallFont  { get; private set; }
    public static GUI.BitmapFont MediumFont { get; private set; }
    public static GUI.BitmapFont LargeFont  { get; private set; }

    // Loaded textures for entity sprites and cursor
    public static TextureAtlas TileAtlas   { get; private set; }
    public static TextureAtlas EntityAtlas { get; private set; }
    public static Texture2D    CursorTex  { get; private set; }

    public StateStack StateStack { get; private set; }

    private RenderTarget2D _renderTarget;

    public Game1()
        : base("50-Mon",
               GameSettings.WindowWidth,  GameSettings.WindowHeight,
               GameSettings.VirtualWidth, GameSettings.VirtualHeight)
    {
        Current = this;
    }

    protected override void Initialize()
    {
        StateStack = new StateStack();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        // 1×1 white pixel
        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });

        SmallFont  = GUI.BitmapFont.CreateSmall(Content.Load<Texture2D>("fonts/small_atlas"));
        MediumFont = GUI.BitmapFont.CreateMedium(Content.Load<Texture2D>("fonts/medium_atlas"));
        LargeFont  = GUI.BitmapFont.CreateLarge(Content.Load<Texture2D>("fonts/large_atlas"));

        TileAtlas   = TextureAtlas.FromGrid(Content.Load<Texture2D>("images/tiles"), GameSettings.TileSize, GameSettings.TileSize);
        EntityAtlas = TextureAtlas.FromGrid(Content.Load<Texture2D>("images/entities"), GameSettings.TileSize, GameSettings.TileSize);
        CursorTex   = Content.Load<Texture2D>("images/cursor");

        _renderTarget = new RenderTarget2D(GraphicsDevice, GameSettings.VirtualWidth, GameSettings.VirtualHeight);

        // Load Pokemon battle sprite textures via EntityDefinitions
        EntityDefinitions.LoadContent(Content);

        SoundManager.LoadContent(Content);

        StateStack.Push(new StartState(this));
    }

    protected override void Update(GameTime gameTime)
    {
        // Tweens fire first so state changes from callbacks are visible to StateStack.Update.
        TweenManager.Instance.Update(gameTime);
        StateStack.Update(gameTime);
        base.Update(gameTime);
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
