using Microsoft.Xna.Framework;

namespace Pokemon.Entities;

public enum Direction { Down, Up, Left, Right }

public static class DirectionExtensions
{
    // Convert a direction to a unit movement vector.
    public static Vector2 ToVector2(this Direction d) => d switch
    {
        Direction.Left  => new Vector2(-1,  0),
        Direction.Right => new Vector2( 1,  0),
        Direction.Up    => new Vector2( 0, -1),
        Direction.Down  => new Vector2( 0,  1),
        _               => Vector2.Zero
    };

}
