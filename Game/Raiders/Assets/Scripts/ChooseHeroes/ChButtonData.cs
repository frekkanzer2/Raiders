using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChButtonData : MonoBehaviour
{

    public CharacterInfo infoConnected;
    public int team;
    private SelectionManager sm;
    private bool isSet = false;

    public void initialize(CharacterInfo ci, int team, SelectionManager sm) {
        this.infoConnected = ci;
        this.team = team;
        this.GetComponent<Image>().sprite = ci.characterFullSprite;
        this.sm = sm;
    }

    public void initialize(CharacterInfo ci, int team) {
        this.infoConnected = ci;
        this.team = team;
        this.GetComponent<Image>().sprite = ci.characterFullSprite;
    }

    public void onPlayerPressed() {
        if (SelectionManager.definitiveLock == true) return;
        if (team == 1 && !sm.canAlphaChoose) return;
        if (team == 2 && !sm.canBetaChoose) return;
        if (isSet) return;
        isSet = true;
        if (team == 1 && sm.canAlphaChoose) {
            sm.isAlphaLocked = false;
            sm.registerCharacterChosen(infoConnected, this, 1);
            this.GetComponent<Image>().color = new Color(150f / 255f, 150f / 255f, 150f / 255f, 0.5f);
        } else if (team == 2 && sm.canBetaChoose) {
            sm.isBetaLocked = false;
            sm.registerCharacterChosen(infoConnected, this, 2);
            this.GetComponent<Image>().color = new Color(150f / 255f, 150f / 255f, 150f / 255f, 0.5f);
        }
        SoundUi.Instance.playAudio(SoundUi.AudioType.Preview_ChooseHero);
    }

    public void resetSprite() {
        isSet = false;
        this.GetComponent<Image>().color = new Color(1, 1, 1, 1);
    }

}
