using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using GMDCore.Graphics;
using GMDCore.GUI;

namespace Pokemon.GUI;

// A Panel combined with a Selection — the standard menu widget.
public sealed class Menu
{
    private readonly Panel _panel;
    private readonly Selection _selection;

    public Menu(float x, float y, float width, float height,
                List<Selection.MenuItem> items, BitmapFont font, Texture2D cursor)
    {
        _panel     = new Panel(x, y, width, height);
        _selection = new Selection(x, y, width, height, items, font, cursor);
    }

    public void Update() => _selection.Update();

    public void Draw(SpriteBatch spriteBatch)
    {
        _panel.Draw(spriteBatch);
        _selection.Draw(spriteBatch);
    }
}
