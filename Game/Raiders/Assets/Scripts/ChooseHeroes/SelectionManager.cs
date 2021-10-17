using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [HideInInspector]
    public ConfirmButton specialButtonA;
    [HideInInspector]
    public ConfirmButton specialButtonB;

    public GameObject blackScreen;
    private Image bscreen;

    [HideInInspector]
    public bool canAlphaChoose = true;
    [HideInInspector]
    public bool isAlphaLocked = false;
    [HideInInspector]
    public bool canBetaChoose = true;
    [HideInInspector]
    public bool isBetaLocked = false;

    [HideInInspector]
    public static bool definitiveLock = false;

    public void setDefinitiveLock() {
        SelectionManager.definitiveLock = true;
        blackScreen.SetActive(true);
        StartCoroutine(startBlackScreen());
    }

    IEnumerator startBlackScreen() {
        yield return new WaitForSeconds(1f);
        bscreen = blackScreen.GetComponent<Image>();
        StartCoroutine(incrementBlackScreen());
    }

    IEnumerator incrementBlackScreen() {
        yield return new WaitForSeconds(0.02f);
        bscreen.color = new Color(bscreen.color.r, bscreen.color.b, bscreen.color.g, bscreen.color.a + 0.05f);
        if (bscreen.color.a <= 0.99f) StartCoroutine(incrementBlackScreen());
        else StartCoroutine(changeRoom());
    }

    IEnumerator changeRoom() {
        yield return new WaitForSeconds(1f);
        List<CharacterInfo> la = GetComponent<SelectionContainer>().teamACharacters;
        List<CharacterInfo> lb = GetComponent<SelectionContainer>().teamBCharacters;
        PlayerPrefs.SetInt("TEAM_DIMENSION", la.Count);
        int index = 0;
        foreach(CharacterInfo ci in la) {
            PlayerPrefs.SetString("TEAM_ALPHA_" + index, ci.characterName);
            index++;
        }
        index = 0;
        foreach (CharacterInfo ci in lb) {
            PlayerPrefs.SetString("TEAM_BETA_" + index, ci.characterName);
            index++;
        }
        SceneManager.LoadScene("BattleScene", LoadSceneMode.Single);
    }

    private void Start() {
        blackScreen.SetActive(false);
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
            abutton.GetComponent<ButtonPreview>().team = 1;
            bbutton.GetComponent<ButtonPreview>().team = 2;
            listPreviewsAlpha.Add(abutton.GetComponent<ButtonPreview>());
            listPreviewsBeta.Add(bbutton.GetComponent<ButtonPreview>());
        }
        GameObject acbutton = GameObject.Instantiate(prefabConfirmButtonAlpha);
        acbutton.transform.SetParent(teamAreferenceToPreviewBtnSlider.transform);
        GameObject bcbutton = GameObject.Instantiate(prefabConfirmButtonBeta);
        bcbutton.transform.SetParent(teamBreferenceToPreviewBtnSlider.transform);
        specialButtonA = acbutton.GetComponent<ConfirmButton>();
        specialButtonA.sm = this;
        specialButtonA.team = 1;
        specialButtonB = bcbutton.GetComponent<ConfirmButton>();
        specialButtonB.sm = this;
        specialButtonB.team = 2;
    }

    public void registerCharacterChosen(CharacterInfo ci, ChButtonData ch, int team) {
        if (team == 1) {
            List<CharacterInfo> l = GetComponent<SelectionContainer>().teamACharacters;
            foreach (ButtonPreview bp in listPreviewsAlpha) {
                if (!bp.isSet && l.Count < 5) {
                    bp.setCharacter(ci.characterMidSprite, ci, ch, this);
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
                    bp.setCharacter(ci.characterMidSprite, ci, ch, this);
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

    public void unregisterCharacterChosen(CharacterInfo ci, int team) {
        if (team == 1) {
            SelectionContainer sc = GetComponent<SelectionContainer>();
            sc.removeCharacter(ci, 1);
            if (sc.teamACharacters.Count < 3) {
                specialButtonA.setCanDeny();
            } else {
                specialButtonA.setCanValidate();
            }
            canAlphaChoose = true;
        } else if (team == 2) {
            SelectionContainer sc = GetComponent<SelectionContainer>();
            sc.removeCharacter(ci, 2);
            if (sc.teamBCharacters.Count < 3) {
                specialButtonB.setCanDeny();
            } else {
                specialButtonB.setCanValidate();
            }
            canBetaChoose = true;
        }
    }

}
