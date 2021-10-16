using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionManager : MonoBehaviour
{

    public GameObject teamAreferenceToCharSlider;
    public GameObject teamAreferenceToPreviewBtnSlider;
    public GameObject teamBreferenceToCharSlider;
    public GameObject teamBreferenceToPreviewBtnSlider;

    public GameObject prefabCharacter;
    public GameObject prefabButtonAlpha;
    public GameObject prefabButtonBeta;
    public GameObject prefabConfirmButtonAlpha;
    public GameObject prefabConfirmButtonBeta;

    private List<ButtonPreview> listPreviewsAlpha = new List<ButtonPreview>();
    private List<ButtonPreview> listPreviewsBeta = new List<ButtonPreview>();

    private ConfirmButton specialButtonA;
    private ConfirmButton specialButtonB;

    [HideInInspector]
    public bool canAlphaChoose = true;
    [HideInInspector]
    public bool canBetaChoose = true;

    private void Start() {
        GetComponent<CharactersLibrary>().init();
        List<CharacterInfo> lib = CharactersLibrary.getLibrary();
        teamAreferenceToCharSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(70.5352f * lib.Count, teamAreferenceToCharSlider.GetComponent<RectTransform>().sizeDelta.y);
        teamBreferenceToCharSlider.GetComponent<RectTransform>().sizeDelta = teamAreferenceToCharSlider.GetComponent<RectTransform>().sizeDelta;
        foreach (CharacterInfo ci in lib) {
            // Spawning characters' previews
            GameObject instanceAlpha = GameObject.Instantiate(prefabCharacter);
            GameObject instanceBeta = GameObject.Instantiate(prefabCharacter);
            instanceAlpha.transform.SetParent(teamAreferenceToCharSlider.transform);
            instanceBeta.transform.SetParent(teamBreferenceToCharSlider.transform);
            ChButtonData alphaChData = instanceAlpha.GetComponent<ChButtonData>();
            ChButtonData betaChData = instanceBeta.GetComponent<ChButtonData>();
            alphaChData.initialize(ci, 1, this);
            betaChData.initialize(ci, 2, this);
        }
        for (int i = 0; i < 5; i++) {
            GameObject abutton = GameObject.Instantiate(prefabButtonAlpha);
            abutton.transform.SetParent(teamAreferenceToPreviewBtnSlider.transform);
            GameObject bbutton = GameObject.Instantiate(prefabButtonBeta);
            bbutton.transform.SetParent(teamBreferenceToPreviewBtnSlider.transform);
            listPreviewsAlpha.Add(abutton.GetComponent<ButtonPreview>());
            listPreviewsBeta.Add(bbutton.GetComponent<ButtonPreview>());
        }
        GameObject acbutton = GameObject.Instantiate(prefabConfirmButtonAlpha);
        acbutton.transform.SetParent(teamAreferenceToPreviewBtnSlider.transform);
        GameObject bcbutton = GameObject.Instantiate(prefabConfirmButtonBeta);
        bcbutton.transform.SetParent(teamBreferenceToPreviewBtnSlider.transform);
        specialButtonA = acbutton.GetComponent<ConfirmButton>();
        specialButtonB = bcbutton.GetComponent<ConfirmButton>();
    }

    public void registerCharacterChosen(CharacterInfo ci, int team) {
        if (team == 1) {
            List<CharacterInfo> l = GetComponent<SelectionContainer>().teamACharacters;
            foreach (ButtonPreview bp in listPreviewsAlpha) {
                if (!bp.isSet && l.Count < 5) {
                    bp.setCharacter(ci.characterMidSprite);
                    l.Add(ci);
                    break;
                }
            }
            if (l.Count >= 3) {
                specialButtonA.setCanValidate();
            }
            if (l.Count == 5) canAlphaChoose = false;
        } else if (team == 2) {
            List<CharacterInfo> l = GetComponent<SelectionContainer>().teamBCharacters;
            foreach (ButtonPreview bp in listPreviewsBeta) {
                if (!bp.isSet && l.Count < 5) {
                    bp.setCharacter(ci.characterMidSprite);
                    l.Add(ci);
                    break;
                }
            }
            if (l.Count >= 3) {
                specialButtonB.setCanValidate();
            }
            if (l.Count == 5) canBetaChoose = false;
        }
    }

}
