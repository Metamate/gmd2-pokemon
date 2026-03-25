using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon;
using Pokemon.Input;
using GMDCore.Graphics;
using GMDCore.GUI;

namespace Pokemon.GUI;

// Vertical menu selection widget. Draws item labels and a cursor next to the selected item.
// Responds to MenuUp / MenuDown and Confirm input events.
public sealed class Selection
{
    public sealed class MenuItem
    {
        public string Text     { get; init; }
        public Action OnSelect { get; init; }
    }

    private readonly List<MenuItem> _items;
    private int _currentIndex;

    public float X      { get; }
    public float Y      { get; }
    public float Width  { get; }
    public float Height { get; }

    private readonly BitmapFont _font;
    private readonly Texture2D  _cursor;

    public Selection(float x, float y, float width, float height,
                     List<MenuItem> items, BitmapFont font, Texture2D cursor)
    {
        X = x; Y = y; Width = width; Height = height;
        _items  = items;
        _font   = font;
        _cursor = cursor;
    }

    public void Update()
    {
        if (_items.Count == 0) return;

        if (GameController.MenuUp)
        {
            _currentIndex = (_currentIndex - 1 + _items.Count) % _items.Count;
            Locator.Audio.PlayBlip();
        }
        else if (GameController.MenuDown)
        {
            _currentIndex = (_currentIndex + 1) % _items.Count;
            Locator.Audio.PlayBlip();
        }
        else if (GameController.Confirm)
        {
            Locator.Audio.PlayBlip();
            _items[_currentIndex].OnSelect?.Invoke();
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        float gapHeight = _items.Count > 0 ? Height / _items.Count : Height;
        float currentY  = Y;

        for (int i = 0; i < _items.Count; i++)
        {
            float textHeight = _font.MeasureString(_items[i].Text).Y;
            float paddedY    = currentY + (gapHeight / 2f) - textHeight / 2f;

            if (i == _currentIndex && _cursor != null)
            {
                spriteBatch.Draw(_cursor, new Vector2(X - GameSettings.CursorWidth - 1, paddedY), Color.White);
            }

            // Centre text within the width
            float textWidth = _font.MeasureString(_items[i].Text).X;
            float centredX  = X + (Width - textWidth) / 2f;
            _font.Draw(spriteBatch, _items[i].Text, new Vector2(centredX, paddedY), Color.White);

            currentY += gapHeight;
        }
    }
}

