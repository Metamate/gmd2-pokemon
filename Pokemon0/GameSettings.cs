namespace Pokemon0;

public static class GameSettings
{
    public const int WindowWidth = 1280;
    public const int WindowHeight = 720;
    public const int VirtualWidth = 384;
    public const int VirtualHeight = 216;
    public const int TileSize = 16;

    // Level dimensions in tiles (matches the visible viewport at 384×216 with 16px tiles)
    public const int MapCols = 24;
    public const int MapRows = 13;

    public const int PlayerStartMapX = 9;
    public const int PlayerStartMapY = 9;

    public const int TallGrassStartRow = 10;

    public static readonly int[] TileGrass = { 45, 46 };
    public const int TileTallGrass = 41;

    // Time (seconds) to tween one tile-step walk
    public const float WalkTweenDuration = 0.5f;
}
