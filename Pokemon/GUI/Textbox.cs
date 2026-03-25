using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Input;
using GMDCore.Graphics;
using GMDCore.GUI;

namespace Pokemon.GUI;

// Displays a text message inside a Panel, one page (up to 3 lines) at a time.
// The player presses Confirm to advance pages. When the last page is dismissed
// the textbox marks itself as closed.
public sealed class Textbox
{
    private readonly Panel _panel;
    private readonly BitmapFont _font;
    private readonly List<string> _allLines;
    private List<string> _displayLines = new();

    private int  _lineIndex;
    private bool _endOfText;
    public  bool IsClosed { get; private set; }

    public float X      { get; }
    public float Y      { get; }
    public float Width  { get; }
    public float Height { get; }

    public Textbox(float x, float y, float width, float height, string text, BitmapFont font)
    {
        X = x; Y = y; Width = width; Height = height;
        _font  = font;
        _panel = new Panel(x, y, width, height);

        _allLines = WrapText(font, text, width - GameSettings.TextboxPadding * 3);
        ShowNextPage();
    }

    private void ShowNextPage()
    {
        if (_endOfText)
        {
            _displayLines.Clear();
            _panel.Toggle();   // hide panel
            IsClosed = true;
            return;
        }

        _displayLines.Clear();
        for (int i = 0; i < GameSettings.TextboxLinesPerPage && _lineIndex < _allLines.Count; i++, _lineIndex++)
            _displayLines.Add(_allLines[_lineIndex]);

        if (_lineIndex >= _allLines.Count)
            _endOfText = true;
    }

    public void Update()
    {
        if (IsClosed) return;
        if (GameController.Confirm)
            ShowNextPage();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _panel.Draw(spriteBatch);

        for (int i = 0; i < _displayLines.Count; i++)
        {
            _font.Draw(spriteBatch, _displayLines[i],
                new Vector2(X + GameSettings.TextboxPadding,
                            Y + GameSettings.TextboxPadding + i * (_font.LineHeight + 2)),
                Color.White);
        }
    }

    // ---- Word-wrap helper ----
    private static List<string> WrapText(BitmapFont font, string text, float maxWidth)
    {
        var lines = new List<string>();
        var words = text.Split(' ');
        var current = string.Empty;

        foreach (var word in words)
        {
            var test = current.Length > 0 ? current + " " + word : word;
            if (font.MeasureString(test).X > maxWidth && current.Length > 0)
            {
                lines.Add(current);
                current = word;
            }
            else
            {
                current = test;
            }
        }

        if (current.Length > 0)
            lines.Add(current);

        return lines;
    }
}
