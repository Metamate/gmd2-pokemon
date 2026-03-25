using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Pokemon.Audio;

// Central audio manager. Music tracks are Songs played via MediaPlayer (one at a time).
// Short sound effects are SoundEffects.
public static class SoundManager
{
    private static Song _fieldMusic;
    private static Song _battleMusic;
    private static Song _introMusic;
    private static Song _victoryMusic;

    private static SoundEffect _blip;
    private static SoundEffect _powerup;
    private static SoundEffect _hit;
    private static SoundEffect _run;
    private static SoundEffect _heal;
    private static SoundEffect _exp;
    private static SoundEffect _levelup;

    public static void LoadContent(ContentManager content)
    {
        _fieldMusic   = content.Load<Song>("sounds/field_music");
        _battleMusic  = content.Load<Song>("sounds/battle_music");
        _introMusic   = content.Load<Song>("sounds/intro");
        _victoryMusic = content.Load<Song>("sounds/victory");

        _blip     = content.Load<SoundEffect>("sounds/blip");
        _powerup  = content.Load<SoundEffect>("sounds/powerup");
        _hit      = content.Load<SoundEffect>("sounds/hit");
        _run      = content.Load<SoundEffect>("sounds/run");
        _heal     = content.Load<SoundEffect>("sounds/heal");
        _exp      = content.Load<SoundEffect>("sounds/exp");
        _levelup  = content.Load<SoundEffect>("sounds/levelup");

        MediaPlayer.IsRepeating = true;
    }

    // ---- Music ----
    public static void PlayFieldMusic()
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_fieldMusic);
    }

    // Pause the field music so it can be resumed later (conceptually; MediaPlayer resumes from start).
    public static void PauseFieldMusic() => MediaPlayer.Pause();

    public static void PlayBattleMusic()
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_battleMusic);
    }

    public static void PlayIntroMusic()
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_introMusic);
    }

    public static void PlayVictoryMusic()
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(_victoryMusic);
    }

    public static void StopMusic() => MediaPlayer.Stop();

    // ---- SFX ----
    public static void PlayBlip()    => _blip?.Play();
    public static void PlayPowerup() => _powerup?.Play();
    public static void PlayHit()     => _hit?.Play();
    public static void PlayRun()     => _run?.Play();
    public static void PlayHeal()    => _heal?.Play();
    public static void PlayExp()     => _exp?.Play();
    public static void PlayLevelup() => _levelup?.Play();
}
