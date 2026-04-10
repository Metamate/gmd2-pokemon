namespace Pokemon.Audio;

public interface IAudio
{
    void PlayIntroMusic();
    void PlayFieldMusic();
    void PlayBattleMusic();
    void PlayVictoryMusic();
    void StopMusic();

    void PlayBlip();
    void PlayPowerup();
    void PlayHit();
    void PlayRun();
    void PlayHeal();
    void PlayExp();
    void PlayLevelup();
}
