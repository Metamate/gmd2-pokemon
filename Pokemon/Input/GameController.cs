using GMDCore;
using Microsoft.Xna.Framework.Input;
using Pokemon.Entities;

namespace Pokemon.Input;

// Abstracts raw keyboard state into named game actions.
// All properties read from the shared GMDCore InputManager.Keyboard.
public static class GameController
{
    // Movement (held)
    public static bool Left  => Core.Input.Keyboard.IsKeyDown(Keys.Left)  || Core.Input.Keyboard.IsKeyDown(Keys.A);
    public static bool Right => Core.Input.Keyboard.IsKeyDown(Keys.Right) || Core.Input.Keyboard.IsKeyDown(Keys.D);
    public static bool Up    => Core.Input.Keyboard.IsKeyDown(Keys.Up)    || Core.Input.Keyboard.IsKeyDown(Keys.W);
    public static bool Down  => Core.Input.Keyboard.IsKeyDown(Keys.Down)  || Core.Input.Keyboard.IsKeyDown(Keys.S);

    // Returns the held movement direction, or null if no direction key is pressed.
    public static Direction? MovementDirection =>
        Left  ? Direction.Left  :
        Right ? Direction.Right :
        Up    ? Direction.Up    :
        Down  ? Direction.Down  :
        null;

    // Confirm / advance text (just-pressed)
    public static bool Confirm => Core.Input.Keyboard.WasKeyJustPressed(Keys.Enter)
                                || Core.Input.Keyboard.WasKeyJustPressed(Keys.Space);

    // Heal shortcut (just-pressed)
    public static bool Heal => Core.Input.Keyboard.WasKeyJustPressed(Keys.P);

    // Menu navigation (just-pressed)
    public static bool MenuUp   => Core.Input.Keyboard.WasKeyJustPressed(Keys.Up)   || Core.Input.Keyboard.WasKeyJustPressed(Keys.W);
    public static bool MenuDown => Core.Input.Keyboard.WasKeyJustPressed(Keys.Down) || Core.Input.Keyboard.WasKeyJustPressed(Keys.S);
}
