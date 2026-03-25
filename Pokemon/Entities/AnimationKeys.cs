namespace Pokemon.Entities;

/// <summary>
/// Compile-time constants for entity animation keys.
/// Centralises the naming convention so typos become build errors rather than silent bugs.
/// </summary>
public static class AnimationKeys
{
    public const string WalkDown  = "walk-down";
    public const string WalkUp    = "walk-up";
    public const string WalkLeft  = "walk-left";
    public const string WalkRight = "walk-right";
    public const string IdleDown  = "idle-down";
    public const string IdleUp    = "idle-up";
    public const string IdleLeft  = "idle-left";
    public const string IdleRight = "idle-right";

    /// <summary>Returns the walk animation key for the given direction.</summary>
    public static string Walk(Direction d) => d switch
    {
        Direction.Up    => WalkUp,
        Direction.Down  => WalkDown,
        Direction.Left  => WalkLeft,
        Direction.Right => WalkRight,
        _               => WalkDown
    };

    /// <summary>Returns the idle animation key for the given direction.</summary>
    public static string Idle(Direction d) => d switch
    {
        Direction.Up    => IdleUp,
        Direction.Down  => IdleDown,
        Direction.Left  => IdleLeft,
        Direction.Right => IdleRight,
        _               => IdleDown
    };
}
