using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.Graphics;

// Utility methods for generating textures programmatically.
public static class TextureFactory
{
    // Creates a solid-color ellipse texture of size (rx*2) x (ry*2).
    // Pixels outside the ellipse are transparent.
    public static Texture2D CreateEllipse(GraphicsDevice gd, int rx, int ry, Color color)
    {
        int w = rx * 2, h = ry * 2;
        var tex  = new Texture2D(gd, w, h);
        var data = new Color[w * h];
        for (int py = 0; py < h; py++)
            for (int px = 0; px < w; px++)
            {
                float dx = (px - rx + 0.5f) / rx;
                float dy = (py - ry + 0.5f) / ry;
                data[py * w + px] = dx * dx + dy * dy <= 1f ? color : Color.Transparent;
            }
        tex.SetData(data);
        return tex;
    }
}
