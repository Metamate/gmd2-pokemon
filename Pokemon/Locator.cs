using GMDCore.Tweening;
using Pokemon.Audio;

namespace Pokemon;

// Service locator. Provides global access to core services without hard-coding
// concrete implementations. Start with null services so nothing crashes before
// real services are registered.
public static class Locator
{
    public static TweenManager Tweens { get; private set; } = new TweenManager();
    public static IAudio        Audio { get; private set; } = new NullAudio();

    public static void Provide(TweenManager tweens) => Tweens = tweens;
    public static void Provide(IAudio audio)        => Audio  = audio;
}
