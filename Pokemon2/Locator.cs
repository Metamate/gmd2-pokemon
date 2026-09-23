using GMDCore.Tweening;

namespace Pokemon2;

// Service locator. Provides global access to core services without hard-coding
// concrete implementations. Start with null services so nothing crashes before
// real services are registered.
public static class Locator
{
    public static ITweenManager Tweens { get; private set; } = new TweenManager();
    public static GameAssets    Assets { get; private set; } = null;

    public static void Provide(ITweenManager tweens) => Tweens = tweens;
    public static void Provide(GameAssets assets)    => Assets = assets;
}
