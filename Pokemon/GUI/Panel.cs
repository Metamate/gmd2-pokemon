using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon.GUI;

/// <summary>
/// A bordered panel background (white outer, dark inner rectangle).
/// Equivalent to the Lua Panel class.
/// </summary>
public sealed class Panel
{
    public float X      { get; set; }
    public float Y      { get; set; }
    public float Width  { get; set; }
    public float Height { get; set; }
    public bool  Visible { get; set; } = true;

    private static readonly Color InnerColor = new(56, 56, 56);

    public Panel(float x, float y, float width, float height)
    {
        X = x; Y = y; Width = width; Height = height;
    }

    public void Toggle() => Visible = !Visible;

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        if (!Visible) return;

        // Outer white border
        spriteBatch.Draw(pixel,
            new Rectangle((int)X, (int)Y, (int)Width, (int)Height),
            Color.White);

        // Inner dark fill
        spriteBatch.Draw(pixel,
            new Rectangle((int)X + 2, (int)Y + 2, (int)Width - 4, (int)Height - 4),
            InnerColor);
    }
}
