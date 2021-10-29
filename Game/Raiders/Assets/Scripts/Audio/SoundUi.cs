using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundUi : MonoBehaviour {

    private static SoundUi _instance;
    public static SoundUi Instance { get { return _instance; } }

    public AudioClip preview_chooseHero;
    public AudioClip preview_removeHero;
    public AudioClip preview_confirmTeam;
    public AudioClip setHeroInCell;
    public AudioClip heroChoiseConfirm1;
    public AudioClip heroChoiseConfirm2;
    public AudioClip startBattle;
    public AudioClip pressedButtonNextTurn;
    public AudioClip newTurnStarted;
    public AudioClip showSpell;

    public enum AudioType {
        Preview_ChooseHero,
        Preview_RemoveHero,
        Preview_ConfirmTeam,
        HeroChoise_SetHeroInCell,
        HeroChoise_Confirm1,
        HeroChoise_Confirm2,
        StartBattle,
        ButtonPressed_NextTurn,
        StartTurn,
        ButtonPressed_Spell
    }

    private void Start() {
        if (SoundUi.Instance == null) SoundUi._instance = this;
    }

    public void playAudio(AudioType type) {
        Debug.Log("Play UI audio call");
        if (type == AudioType.Preview_ChooseHero)
            play(preview_chooseHero, 1);
        else if (type == AudioType.Preview_RemoveHero)
            play(preview_removeHero, 1);
        else if (type == AudioType.Preview_ConfirmTeam)
            play(preview_confirmTeam, 1);
        else if (type == AudioType.HeroChoise_Confirm1)
            play(heroChoiseConfirm1, 1);
        else if (type == AudioType.HeroChoise_Confirm2)
            play(heroChoiseConfirm2, 1);
        else if (type == AudioType.HeroChoise_SetHeroInCell)
            play(setHeroInCell, 1);
        else if (type == AudioType.StartBattle)
            play(startBattle, 2);
        else if (type == AudioType.StartTurn)
            play(newTurnStarted, 2);
        else if (type == AudioType.ButtonPressed_NextTurn)
            play(pressedButtonNextTurn, 1);
        else if (type == AudioType.ButtonPressed_Spell)
            play(showSpell, 1);
        else
            Debug.LogError("NOT PROVIDED AUDIO");
    }

    private void play(AudioClip ac, int channel) {
        channel--;
        Debug.LogWarning("Asking to play " + ac.name);
        AudioSource[] asources = this.gameObject.GetComponents<AudioSource>(); // returns 0 and 1 AudioSources
        AudioSource asource = asources[channel];
        asource.clip = ac;
        asource.Play();
    }
    
}
