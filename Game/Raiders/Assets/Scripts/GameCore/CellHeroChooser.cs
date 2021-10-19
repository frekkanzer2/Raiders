using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellHeroChooser : MonoBehaviour
{

    private CharacterInfo connectedInfo;
    private PreparationManager pointer;
    private int team;
    public GameObject imageContainer;
    
    public void initialize(CharacterInfo ch, PreparationManager pointer, int team) {
        this.pointer = pointer;
        this.team = team;
        connectedInfo = ch;
        imageContainer.GetComponent<Image>().sprite = ch.characterMidSprite;
    }

    public CharacterInfo getCharacterInfo() {
        return this.connectedInfo;
    }

    public void OnPress() {
        Debug.Log("Pressing " + this.gameObject.name);
        pointer.OnCellChoosePress(team, connectedInfo, this);
        this.gameObject.SetActive(false);
    }

}
