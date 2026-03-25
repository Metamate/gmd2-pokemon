using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon.Battle;

// Renders a Pokemon battle sprite with position, opacity, and blink support.
public sealed class BattleSprite
{
    public Texture2D Texture { get; }
    public float X       { get; set; }
    public float Y       { get; set; }
    public float Opacity { get; set; } = 1f;

    // When true the sprite is hidden (simulates the Lua "flash white" blink).
    public bool Blinking { get; set; }

    public BattleSprite(Texture2D texture, float x, float y)
    {
        Texture = texture;
        X = x;
        Y = y;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Blinking) return;
        spriteBatch.Draw(Texture, new Vector2(X, Y), Color.White * Opacity);
    }
}
