using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pokemon.Battle;

// Renders a Pokemon battle sprite with position, opacity, and blink support.
// 
// The Lua version used a custom GLSL shader to flash the sprite pure white.
// MonoGame SpriteBatch has no built-in shader support so the blink effect is
// approximated by toggling visibility (hide when blinking = true), which conveys
// the same "attacking" feedback.
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
        if (Blinking) return;  // hidden during blink frames
        spriteBatch.Draw(Texture, new Vector2(X, Y), Color.White * Opacity);
    }
}
