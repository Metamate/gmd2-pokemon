using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace Pokemon.Audio;

// Concrete audio service. Load content first, then register with Locator.Provide(audio).
public sealed class SoundManager : IAudio
{
    private Song _fieldMusic;
    private Song _battleMusic;
    private Song _introMusic;
    private Song _victoryMusic;

    private SoundEffect _blip;
    private SoundEffect _powerup;
    private SoundEffect _hit;
    private SoundEffect _run;
    private SoundEffect _heal;
    private SoundEffect _exp;
    private SoundEffect _levelup;

    public void LoadContent(ContentManager content)
    {
        _fieldMusic   = content.Load<Song>("sounds/field_music");
        _battleMusic  = content.Load<Song>("sounds/battle_music");
        _introMusic   = content.Load<Song>("sounds/intro");
        _victoryMusic = content.Load<Song>("sounds/victory");

        _blip    = content.Load<SoundEffect>("sounds/blip");
        _powerup = content.Load<SoundEffect>("sounds/powerup");
        _hit     = content.Load<SoundEffect>("sounds/hit");
        _run     = content.Load<SoundEffect>("sounds/run");
        _heal    = content.Load<SoundEffect>("sounds/heal");
        _exp     = content.Load<SoundEffect>("sounds/exp");
        _levelup = content.Load<SoundEffect>("sounds/levelup");
    }

    private static void PlayMusic(Song song)
    {
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Play(song);
    }

    public void PlayFieldMusic()   => PlayMusic(_fieldMusic);
    public void PlayBattleMusic()  => PlayMusic(_battleMusic);
    public void PlayIntroMusic()   => PlayMusic(_introMusic);
    public void PlayVictoryMusic() => PlayMusic(_victoryMusic);
    public void StopMusic()        => MediaPlayer.Stop();

    public void PlayBlip()    => _blip?.Play();
    public void PlayPowerup() => _powerup?.Play();
    public void PlayHit()     => _hit?.Play();
    public void PlayRun()     => _run?.Play();
    public void PlayHeal()    => _heal?.Play();
    public void PlayExp()     => _exp?.Play();
    public void PlayLevelup() => _levelup?.Play();
}
