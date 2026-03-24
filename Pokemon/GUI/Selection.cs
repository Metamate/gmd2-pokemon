using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pokemon.Audio;
using Pokemon.Input;

namespace Pokemon.GUI;

/// <summary>
/// Vertical menu selection widget. Draws item labels and a cursor next to the selected item.
/// Responds to MenuUp / MenuDown and Confirm input events.
/// Equivalent to the Lua Selection class.
/// </summary>
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
            AudioHelper.PlayBlipSafe();
        }
        else if (GameController.MenuDown)
        {
            _currentIndex = (_currentIndex + 1) % _items.Count;
            AudioHelper.PlayBlipSafe();
        }
        else if (GameController.Confirm)
        {
            AudioHelper.PlayBlipSafe();
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

/// <summary>Thin helper so SoundManager can be called without crashing if it's not loaded.</summary>
file static class AudioHelper
{
    public static void PlayBlipSafe()
    {
        try { SoundManager.PlayBlip(); } catch { /* audio not loaded yet */ }
    }
}
