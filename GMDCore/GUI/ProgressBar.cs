using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.GUI;

// A simple filled progress bar with a black outline.
public sealed class ProgressBar
{
    public float X      { get; set; }
    public float Y      { get; set; }
    public float Width  { get; set; }
    public float Height { get; set; }
    public Color BarColor { get; set; }

    public float Value { get; set; }
    public float Max   { get; set; }

    public ProgressBar(float x, float y, float width, float height, Color color, float value, float max)
    {
        X = x; Y = y; Width = width; Height = height;
        BarColor = color;
        Value    = value;
        Max      = max;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        float fillWidth = Max > 0 ? (Value / Max) * Width : 0;

        if (fillWidth > 0)
        {
            spriteBatch.Draw(pixel,
                new Rectangle((int)X, (int)Y, (int)fillWidth, (int)Height),
                BarColor);
        }

        DrawOutline(spriteBatch, pixel);
    }

    private void DrawOutline(SpriteBatch spriteBatch, Texture2D pixel)
    {
        int x = (int)X, y = (int)Y, w = (int)Width, h = (int)Height;
        spriteBatch.Draw(pixel, new Rectangle(x,         y,         w, 1), Color.Black); // top
        spriteBatch.Draw(pixel, new Rectangle(x,         y + h - 1, w, 1), Color.Black); // bottom
        spriteBatch.Draw(pixel, new Rectangle(x,         y,         1, h), Color.Black); // left
        spriteBatch.Draw(pixel, new Rectangle(x + w - 1, y,         1, h), Color.Black); // right
    }
}
