using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.Graphics;

/// <summary>
/// A pixel-perfect bitmap font backed by a pre-generated glyph atlas.
/// The atlas is rendered without anti-aliasing (1-bit per pixel) so glyphs stay
/// crisp when the render-target is upscaled with point filtering.
///
/// Atlas layout: printable ASCII 32–126 (95 chars), 16 columns per row,
/// each character occupies a cell of CellW × CellH pixels.
/// Per-character advance widths are stored separately (variable-width font).
/// </summary>
public sealed class BitmapFont
{
    private readonly Texture2D _atlas;
    private readonly int       _cellW;
    private readonly int       _cellH;
    private readonly int[]     _advances;   // advance width per character (index = codepoint - 32)

    private const int FirstChar = 32;
    private const int Cols      = 16;

    public int LineHeight => _cellH;

    // ---- Advance-width tables generated from font.ttf (04b03) ----
    private static readonly int[] AdvancesSmall  = { 4,2,4,6,5,6,6,2,3,3,4,4,3,4,2,6,5,3,5,5,5,5,5,5,5,5,2,2,4,4,4,5,6,5,5,4,5,4,4,5,5,4,5,5,4,6,5,5,5,5,5,5,4,5,5,6,5,5,4,3,6,3,4,5,3,5,5,4,5,5,4,5,5,2,3,5,2,6,5,5,5,5,4,5,4,5,5,6,4,5,5,4,2,4,5 };
    private static readonly int[] AdvancesMedium = { 8,4,8,12,10,12,12,4,6,6,8,8,6,8,4,12,10,6,10,10,10,10,10,10,10,10,4,4,8,8,8,10,12,10,10,8,10,8,8,10,10,8,10,10,8,12,10,10,10,10,10,10,8,10,10,12,10,10,8,6,12,6,8,10,6,10,10,8,10,10,8,10,10,4,6,10,4,12,10,10,10,10,8,10,8,10,10,12,8,10,10,8,4,8,10 };
    private static readonly int[] AdvancesLarge  = { 16,8,16,24,20,24,24,8,12,12,16,16,12,16,8,24,20,12,20,20,20,20,20,20,20,20,8,8,16,16,16,20,24,20,20,16,20,16,16,20,20,16,20,20,16,24,20,20,20,20,20,20,16,20,20,24,20,20,16,12,24,12,16,20,12,20,20,16,20,20,16,20,20,8,12,20,8,24,20,20,20,20,16,20,16,20,20,24,16,20,20,16,8,16,20 };

    public static BitmapFont CreateSmall(Texture2D atlas)  => new(atlas, 6,  8,  AdvancesSmall);
    public static BitmapFont CreateMedium(Texture2D atlas) => new(atlas, 12, 16, AdvancesMedium);
    public static BitmapFont CreateLarge(Texture2D atlas)  => new(atlas, 24, 32, AdvancesLarge);

    private BitmapFont(Texture2D atlas, int cellW, int cellH, int[] advances)
    {
        _atlas    = atlas;
        _cellW    = cellW;
        _cellH    = cellH;
        _advances = advances;
    }

    /// <summary>Draw a string at the given position.</summary>
    public void Draw(SpriteBatch spriteBatch, string text, Vector2 position, Color color)
    {
        float x = position.X, startX = position.X, y = position.Y;
        foreach (char c in text)
        {
            if (c == '\n') { x = startX; y += _cellH + 1; continue; }
            int idx = c - FirstChar;
            if (idx < 0 || idx >= _advances.Length) { x += _advances[0]; continue; }
            int col = idx % Cols;
            int row = idx / Cols;
            spriteBatch.Draw(_atlas, new Vector2(x, y),
                new Rectangle(col * _cellW, row * _cellH, _cellW, _cellH), color);
            x += _advances[idx];
        }
    }

    /// <summary>Measure the pixel dimensions of a string.</summary>
    public Vector2 MeasureString(string text)
    {
        float lineW = 0f, maxW = 0f, h = _cellH;
        foreach (char c in text)
        {
            if (c == '\n')
            {
                if (lineW > maxW) maxW = lineW;
                lineW = 0;
                h += _cellH + 1;
                continue;
            }
            int idx = c - FirstChar;
            lineW += idx >= 0 && idx < _advances.Length ? _advances[idx] : _advances[0];
        }
        if (lineW > maxW) maxW = lineW;
        return new Vector2(maxW, h);
    }
}
