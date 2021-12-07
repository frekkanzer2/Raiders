using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DUNSelectionManager : SelectionManager
{

    private List<ButtonPreview> listPreviewsAlpha = new List<ButtonPreview>();
    private Image bscreen;

    public override void setDefinitiveLock() {
        DUNSelectionManager.definitiveLock = true;
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
        PlayerPrefs.SetInt("TEAM_DIMENSION", la.Count);
        int index = 0;
        foreach(CharacterInfo ci in la) {
            Debug.Log(ci.characterName + " TEAM " + 1);
            PlayerPrefs.SetString("TEAM_ALPHA_" + index, ci.characterName);
            PlayerPrefs.DeleteKey("TEAM_BETA_" + index);
            index++;
        }
        index = 0;
        Debug.LogWarning("GAMMA CHECKPOINT");
        DungeonSave ds = new DungeonSave();
        ds.saveChosenDungeon(GetComponent<DungeonChoosePanel>().getSelectedDungeonID());
        ds.saveDungeonRoom(1);
        SceneManager.LoadScene("DungeonBattleScene", LoadSceneMode.Single);
    }

    private void Start() {
        blackScreen.SetActive(false);
        GetComponent<CharactersLibrary>().init();
        List<CharacterInfo> lib = CharactersLibrary.getLibrary();
        teamAreferenceToCharSlider.GetComponent<RectTransform>().sizeDelta = new Vector2(70.5352f * (lib.Count - CharactersLibrary.getNumberOfEvocations()), teamAreferenceToCharSlider.GetComponent<RectTransform>().sizeDelta.y);
        foreach (CharacterInfo ci in lib) {
            if (ci.isEvocation) continue;
            // Spawning characters' previews
            GameObject instanceAlpha = GameObject.Instantiate(prefabCharacter);
            instanceAlpha.transform.SetParent(teamAreferenceToCharSlider.transform);
            instanceAlpha.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            ChButtonData alphaChData = instanceAlpha.GetComponent<ChButtonData>();
            alphaChData.initialize(ci, 1, this);
        }
        for (int i = 0; i < 5; i++) {
            GameObject abutton = GameObject.Instantiate(prefabButtonAlpha);
            abutton.transform.SetParent(teamAreferenceToPreviewBtnSlider.transform);
            abutton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            abutton.GetComponent<ButtonPreview>().team = 1;
            listPreviewsAlpha.Add(abutton.GetComponent<ButtonPreview>());
        }
        GameObject acbutton = GameObject.Instantiate(prefabConfirmButtonAlpha);
        acbutton.transform.SetParent(teamAreferenceToPreviewBtnSlider.transform);
        acbutton.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        specialButtonA = acbutton.GetComponent<ConfirmButton>();
        specialButtonA.sm = this;
        specialButtonA.team = 1;
    }

    public override void registerCharacterChosen(CharacterInfo ci, ChButtonData ch, int team) {
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
        }
    }

    public override void unregisterCharacterChosen(CharacterInfo ci, int team) {
        if (team == 1) {
            SelectionContainer sc = GetComponent<SelectionContainer>();
            sc.removeCharacter(ci, 1);
            if (sc.teamACharacters.Count < 3) {
                specialButtonA.setCanDeny();
            } else {
                specialButtonA.setCanValidate();
            }
            canAlphaChoose = true;
        }
    }

}
