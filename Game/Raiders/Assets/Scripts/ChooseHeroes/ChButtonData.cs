using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChButtonData : MonoBehaviour
{

    public CharacterInfo infoConnected;
    public int team;
    private SelectionManagerGeneric sm;
    private bool isSet = false;

    public void initialize(CharacterInfo ci, int team, SelectionManagerGeneric sm) {
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
        if (SelectionManagerGeneric.definitiveLock == true) return;
        if (team == 1 && !sm.canAlphaChoose) return;
        if (team == 2 && !sm.canBetaChoose) return;
        if (isSet) return;
        isSet = true;
        Debug.Log("OK");
        if (team == 1 && sm.canAlphaChoose) {
            Debug.Log("OK");
            sm.isAlphaLocked = false;
            Debug.Log("OK");
            sm.registerCharacterChosen(infoConnected, this, 1);
            Debug.Log("OK");
            this.GetComponent<Image>().color = new Color(150f / 255f, 150f / 255f, 150f / 255f, 0.5f);
            Debug.Log("OK");
        } else if (team == 2 && sm.canBetaChoose) {
            sm.isBetaLocked = false;
            sm.registerCharacterChosen(infoConnected, this, 2);
            this.GetComponent<Image>().color = new Color(150f / 255f, 150f / 255f, 150f / 255f, 0.5f);
        }
        Debug.Log("OK");
        SoundUi.Instance.playAudio(SoundUi.AudioType.Preview_ChooseHero);
    }

    public void resetSprite() {
        isSet = false;
        this.GetComponent<Image>().color = new Color(1, 1, 1, 1);
    }

}
