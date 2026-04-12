namespace Pokemon.GUI;

public enum Anchor
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    Center,
    TopCenter,
    BottomCenter
}

public static class Layout
{
    public static (float X, float Y) GetPosition(Anchor anchor, float width, float height, float offsetX = 0, float offsetY = 0)
    {
        float x = offsetX;
        float y = offsetY;

        switch (anchor)
        {
            case Anchor.TopRight:
                x += GameSettings.VirtualWidth - width;
                break;
            case Anchor.BottomLeft:
                y += GameSettings.VirtualHeight - height;
                break;
            case Anchor.BottomRight:
                x += GameSettings.VirtualWidth - width;
                y += GameSettings.VirtualHeight - height;
                break;
            case Anchor.Center:
                x += (GameSettings.VirtualWidth - width) / 2;
                y += (GameSettings.VirtualHeight - height) / 2;
                break;
            case Anchor.TopCenter:
                x += (GameSettings.VirtualWidth - width) / 2;
                break;
            case Anchor.BottomCenter:
                x += (GameSettings.VirtualWidth - width) / 2;
                y += GameSettings.VirtualHeight - height;
                break;
        }

        return (x, y);
    }
}
