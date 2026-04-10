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
    // Static accessors so states can reach shared resources via Game1.Current.
    public static Game1 Current { get; private set; }

    // Three font sizes (pixel-perfect bitmap fonts, no AA).
    public static BitmapFont SmallFont  { get; private set; }
    public static BitmapFont MediumFont { get; private set; }
    public static BitmapFont LargeFont  { get; private set; }

    // Loaded textures for entity sprites and cursor
    public static TextureAtlas TileAtlas   { get; private set; }
    public static TextureAtlas EntityAtlas { get; private set; }
    public static Texture2D    CursorTex  { get; private set; }

    // Shared shadow ellipse texture (used on title screen and in battle)
    public static Texture2D ShadowTex { get; private set; }

    private RenderTarget2D _renderTarget;

    public Game1()
        : base("VIAMon",
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

        SmallFont  = BitmapFont.CreateSmall(Content.Load<Texture2D>("fonts/small_atlas"));
        MediumFont = BitmapFont.CreateMedium(Content.Load<Texture2D>("fonts/medium_atlas"));
        LargeFont  = BitmapFont.CreateLarge(Content.Load<Texture2D>("fonts/large_atlas"));

        TileAtlas   = TextureAtlas.FromGrid(Content.Load<Texture2D>("images/tiles"), GameSettings.TileSize, GameSettings.TileSize);
        EntityAtlas = TextureAtlas.FromGrid(Content.Load<Texture2D>("images/entities"), GameSettings.TileSize, GameSettings.TileSize);
        CursorTex   = Content.Load<Texture2D>("images/cursor");
        ShadowTex   = TextureFactory.CreateEllipse(GraphicsDevice, 72, 24, new Color(45, 184, 45, 124));

        _renderTarget = new RenderTarget2D(GraphicsDevice, GameSettings.VirtualWidth, GameSettings.VirtualHeight);

        // Species data must load before ContentLoader (sprite paths are derived from it)
        PokemonDefinitions.LoadContent(Content);
        ContentLoader.LoadContent(Content);

        var audio = new SoundManager();
        audio.LoadContent(Content);
        Locator.Provide(audio);

        StateStack.Push(new StartState(StateStack));
    }

    protected override void Update(GameTime gameTime)
    {
        // Tweens fire first so state changes from callbacks are visible to StateStack.Update.
        Locator.Tweens.Update(gameTime);
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
