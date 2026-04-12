namespace Pokemon;

public static class GameSettings
{
    public const int WindowWidth  = 1280;
    public const int WindowHeight = 720;
    public const int VirtualWidth  = 384;
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

    // 1-in-N chance of a random encounter when stepping into tall grass
    public const int EncounterChance = 10;

    // Pokemon battle
    public const int PlayerStartLevel   = 5;
    public const int OpponentLevelMin   = 2;
    public const int OpponentLevelMax   = 6; // exclusive upper bound

    // Battle intro slide-in duration (seconds)
    public const float BattleSlideInDuration = 1f;

    // Battle attack animation timing (seconds)
    public const float AttackBlinkInterval  = 0.1f;
    public const int   AttackBlinkCount     = 6;
    public const float HpTweenDuration      = 0.5f;
    public const float ExpTweenDelay        = 1.5f;
    public const float ExpTweenDuration     = 0.5f;
    public const float FaintTweenDuration   = 0.2f;

    // Fade transition duration (seconds)
    public const float FadeDuration = 1f;

    // GUI
    public const int   TextboxPadding    = 4;  // inner padding from panel edge
    public const int   TextboxLinesPerPage = 3;

    // Selection menu cursor sprite size
    public const int CursorWidth  = 8;
}
