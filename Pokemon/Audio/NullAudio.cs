namespace Pokemon.Audio;

// No-op audio service used before real audio is registered.
// Prevents null checks at every call site.
public sealed class NullAudio : IAudio
{
    public void PlayIntroMusic()  { }
    public void PlayFieldMusic()  { }
    public void PlayBattleMusic() { }
    public void PlayVictoryMusic(){ }
    public void StopMusic()       { }

    public void PlayBlip()    { }
    public void PlayPowerup() { }
    public void PlayHit()     { }
    public void PlayRun()     { }
    public void PlayHeal()    { }
    public void PlayExp()     { }
    public void PlayLevelup() { }
}
