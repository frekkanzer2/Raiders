using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPreview : MonoBehaviour
{

    public GameObject charImage;
    public GameObject statusImage;

    private SelectionManager sm;
    public CharacterInfo connectedInfo;
    public Sprite noChar;
    public Sprite empty;
    public Sprite idk;
    public int team;
    private ChButtonData connectedCharacter;

    [HideInInspector]
    public bool isSet = false;

    // Start is called before the first frame update
    void Start()
    {
        charImage.GetComponent<Image>().sprite = noChar;
        statusImage.GetComponent<Image>().sprite = idk;
    }

    public void setCharacter(Sprite input, CharacterInfo ci, ChButtonData ch, SelectionManager sm) {
        charImage.GetComponent<Image>().sprite = input;
        statusImage.GetComponent<Image>().sprite = empty;
        connectedInfo = ci;
        this.sm = sm;
        this.connectedCharacter = ch;
        isSet = true;
    }

    public void removeCharacter() {
        charImage.GetComponent<Image>().sprite = noChar;
        statusImage.GetComponent<Image>().sprite = idk;
        this.connectedCharacter = null;
        connectedInfo = null;
        isSet = false;
    }

    public void onPreviewPressed() {
        if (SelectionManager.definitiveLock == true) return;
        if (!isSet) return;
        if (team == 1 && sm.isAlphaLocked) sm.specialButtonA.setCanValidate();
        if (team == 2 && sm.isBetaLocked) sm.specialButtonB.setCanValidate();
        SoundUi.Instance.playAudio(SoundUi.AudioType.Preview_RemoveHero);
        connectedCharacter.resetSprite();
        sm.unregisterCharacterChosen(connectedInfo, team);
        removeCharacter();
    }

}
