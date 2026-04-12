using System;
using GMDCore.Input;
using GMDCore.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GMDCore;

public class Core : Game
{
    private readonly int _virtualWidth;
    private readonly int _virtualHeight;
    protected GraphicsDeviceManager Graphics;
    // Where the final virtual-resolution image should be drawn inside the window.
    // If the window aspect ratio differs from the virtual one, this rectangle is
    // centered and leaves black bars around it.
    public static Rectangle DestinationRectangle { get; private set; }
    public SpriteBatch SpriteBatch { get; set; }
    public static InputManager Input { get; set; } = new();
    public StateStack StateStack { get; protected set; }

    // 1×1 white pixel texture for drawing solid-color rectangles.
    public static Texture2D Pixel { get; private set; }

    public Core(string title, int windowWidth, int windowHeight, int virtualWidth, int virtualHeight)
    {
        _virtualWidth = virtualWidth;
        _virtualHeight = virtualHeight;
        Graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = windowWidth,
            PreferredBackBufferHeight = windowHeight,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.Title = title;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += (s, e) => UpdatePresentation();
    }

    protected override void Initialize()
    {
        SpriteBatch = new SpriteBatch(GraphicsDevice);
        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData(new[] { Color.White });
        UpdatePresentation();
        base.Initialize();
    }

    // The engine owns the per-frame order: input first, then game logic.
    // Derived games override UpdateGame instead of Update so they always see
    // the newest input snapshot.
    protected sealed override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Input.Update();
        UpdateGame(gameTime);

        base.Update(gameTime);
    }

    protected virtual void UpdateGame(GameTime gameTime) { }

    // Standard draw setup for game states: draw in virtual coordinates with
    // point sampling. Game1 handles scaling the final render target to the window.
    public static void BeginDraw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
    }

    // Recompute the centered destination rectangle that preserves the virtual
    // aspect ratio inside the current window.
    private void UpdatePresentation()
    {
        float screenWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
        float screenHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        float currentWidth, currentHeight;

        if (screenWidth / _virtualWidth > screenHeight / _virtualHeight)
        {
            float aspect = screenHeight / _virtualHeight;
            currentWidth = aspect * _virtualWidth;
            currentHeight = screenHeight;
        }
        else
        {
            float aspect = screenWidth / _virtualWidth;
            currentWidth = screenWidth;
            currentHeight = aspect * _virtualHeight;
        }

        DestinationRectangle = new Rectangle(
            (int)(screenWidth / 2 - currentWidth / 2),
            (int)(screenHeight / 2 - currentHeight / 2),
            (int)currentWidth,
            (int)currentHeight);

        GraphicsDevice.Viewport = new()
        {
            X = DestinationRectangle.X,
            Y = DestinationRectangle.Y,
            Width = DestinationRectangle.Width,
            Height = DestinationRectangle.Height,
            MinDepth = 0,
            MaxDepth = 1,
        };
    }
}
