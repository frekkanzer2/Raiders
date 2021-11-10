using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnsManager : MonoBehaviour
{

    private static TurnsManager _instance;
    public static TurnsManager Instance { get { return _instance; } }

    public static bool isGameStarted = false;
    public static Character active;
    [HideInInspector]
    public List<Character> turns = new List<Character>();
    [HideInInspector]
    public List<Tuple<GameObject, Character, CharacterInfo>> relations = new List<Tuple<GameObject, Character, CharacterInfo>>();
	[HideInInspector]
	public static List<Tuple<Character, Block>> spawnPositions = new List<Tuple<Character, Block>>();

    public GameObject nextTurnButton;
    public GameObject newTurnAnnouncer;
    public GameObject turnsListContainer;
    public GameObject alphaSpace;
    public GameObject betaSpace;
    public GameObject prefabPreviewCardAlpha;
    public GameObject prefabPreviewCardBeta;
    public GameObject injectToChar_prefabNumberDisplayer;
    public GameObject popupSpell;

    private float timeLeft = 3f;

    private void Start() {
        if (TurnsManager.Instance == null) TurnsManager._instance = this;
    }

    private void Update() {
        if (isGameStarted) {
            timeLeft -= Time.deltaTime;
            if (timeLeft < 0) {
                nextTurnButton.SetActive(true);
            }
        }
    }

    public void addRelation(GameObject obj, CharacterInfo ci) {
        relations.Add(new Tuple<GameObject, Character, CharacterInfo>(obj, obj.GetComponent<Character>(), ci));
    }

    public void removeRelation(Character c) {
        Tuple<GameObject, Character, CharacterInfo> toDel = null;
        foreach (Tuple<GameObject, Character, CharacterInfo> t in relations) {
            if (t.Item2.name == c.name && t.Item2.team == c.team) {
                toDel = t;
                break;
            }
        }
        if (toDel != null)
            relations.Remove(toDel);
    }

    public Character getCharacterInTurns(string name, int team) {
        foreach(Character c in turns) {
            if (c.name == name && c.team == team) return c;
        }
        return null;
    }

    private bool hasInitialized = false;

    public void initialize() {
        if (hasInitialized) return;
        hasInitialized = true;
        List<Character> first = new List<Character>();
        List<Character> second = new List<Character>();
        Debug.Log("init with n chars in total: " + turns.Count);
        foreach (Character c in turns) {
            if (c.team == 1) first.Add(c);
            if (c.team == 2) second.Add(c);
        }
        IniComparer ic = new IniComparer();
        first.Sort(ic);
        second.Sort(ic);
        // sum of ini
        int first_ini = 0, second_ini = 0;
        for (int i = 0; i < first.Count; i++) {
            first_ini += first[i].GetComponent<Character>().ini;
            second_ini += second[i].GetComponent<Character>().ini;
        }
        turns.Clear();
        if (first_ini == second_ini) {
            int choise = UnityEngine.Random.Range(0, 2);
            if (choise == 0) first_ini++;
            else second_ini++;
        }
        if (first_ini > second_ini)
            for (int i = 0; i < first.Count; i++) {
                turns.Add(first[i]);
                turns.Add(second[i]);
            }
        else
            for (int i = 0; i < first.Count; i++) {
                turns.Add(second[i]);
                turns.Add(first[i]);
            }
    }

    public void OnStartGame() {
        isGameStarted = true;
        foreach (Character c in turns) {
            c.setupSOS(injectToChar_prefabNumberDisplayer);
            if (c.team == 1) {
                GameObject card = Instantiate(prefabPreviewCardAlpha);
                c.connectedPreview = card;
                card.transform.SetParent(turnsListContainer.transform);
                card.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite = 
	                GetComponent<CharactersLibrary>().getCharacterInfoByName(c.name).characterMidSprite;
	            card.GetComponent<RectTransform>().localScale = new Vector3(0.2f,0.2f,0.2f);
            } else if (c.team == 2) {
                GameObject card = Instantiate(prefabPreviewCardBeta);
                c.connectedPreview = card;
                card.transform.SetParent(turnsListContainer.transform);
                card.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite =
	                GetComponent<CharactersLibrary>().getCharacterInfoByName(c.name).characterMidSprite;
	            card.GetComponent<RectTransform>().localScale = new Vector3(0.2f,0.2f,0.2f);
            }
	        spawnPositions.Add(new Tuple<Character, Block>(c, c.connectedCell.GetComponent<Block>()));
        }
        active = turns[0];
        Debug.Log(active);
        StartActiveCharTurn(false);
    }

    public void injectCharacter(Character caster, Character c) {
        c.setupSOS(injectToChar_prefabNumberDisplayer);
        GameObject card = null;
        if (c.team == 1) {
            card = Instantiate(prefabPreviewCardAlpha);
            c.connectedPreview = card;
            card.transform.SetParent(turnsListContainer.transform);
            card.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite =
                GetComponent<CharactersLibrary>().getCharacterInfoByName(c.name).characterMidSprite;
            card.GetComponent<RectTransform>().localScale = new Vector3(0.2f, 0.2f, 0.2f);
        } else if (c.team == 2) {
            card = Instantiate(prefabPreviewCardBeta);
            c.connectedPreview = card;
            card.transform.SetParent(turnsListContainer.transform);
            card.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Image>().sprite =
                GetComponent<CharactersLibrary>().getCharacterInfoByName(c.name).characterMidSprite;
            card.GetComponent<RectTransform>().localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
        turns.Insert(1, c);
        card.transform.SetSiblingIndex(1);
    }

    public void OnNextTurnPressed() {
        SoundUi.Instance.playAudio(SoundUi.AudioType.ButtonPressed_NextTurn);
        if (active.isMoving || active.isForcedMoving) return;
        Character endTurnCh = turns[0];
        turns.Remove(endTurnCh);
        endTurnCh.turnPassed();
        turns.Add(endTurnCh);
        Character newTurnCh = turns[0];
        active = newTurnCh;
        StartActiveCharTurn(true);
    }

    public void OnSkipTurn() {
        Character endTurnCh = turns[0];
        turns.Remove(endTurnCh);
        endTurnCh.turnPassed();
        turns.Add(endTurnCh);
        Character newTurnCh = turns[0];
        active = newTurnCh;
        StartActiveCharTurn(true);
    }

    private void StartActiveCharTurn(bool scrollPreviews) {
        timeLeft = 3f;
        nextTurnButton.SetActive(false);
        if (active.team == 1)
            alphaSpace.GetComponent<StatsPanel>().set(active, GetComponent<CharactersLibrary>());
        else if (active.team == 2)
            betaSpace.GetComponent<StatsPanel>().set(active, GetComponent<CharactersLibrary>());
        if (active.team == 1) {
            alphaSpace.SetActive(true);
            betaSpace.SetActive(false);
        } else if (active.team == 2) {
            alphaSpace.SetActive(false);
            betaSpace.SetActive(true);
        }
        if (scrollPreviews) {
            GameObject preview = turnsListContainer.transform.GetChild(0).gameObject;
            preview.transform.SetParent(turnsListContainer.transform.parent);
            preview.transform.SetParent(turnsListContainer.transform);
        }
        SoundUi.Instance.playAudio(SoundUi.AudioType.StartTurn);
        StartCoroutine(displayNewHeroName());
        active.newTurn();
    }

    IEnumerator displayNewHeroName() {
        GameObject labelObj = newTurnAnnouncer.transform.GetChild(0).gameObject;
        GameObject charNameObj = newTurnAnnouncer.transform.GetChild(1).GetChild(1).gameObject;
        GameObject leftPanelObj = newTurnAnnouncer.transform.GetChild(1).GetChild(0).gameObject;
        GameObject rightPanelObj = newTurnAnnouncer.transform.GetChild(1).GetChild(2).gameObject;
        labelObj.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
        charNameObj.GetComponent<TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
        charNameObj.GetComponent<TextMeshProUGUI>().text = active.name;
        leftPanelObj.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        rightPanelObj.GetComponent<Image>().color = new Color(1, 1, 1, 0);
        newTurnAnnouncer.SetActive(true);
        StartCoroutine(display_NewTurn(labelObj));
        StartCoroutine(display_Name(charNameObj));
        StartCoroutine(display_LateralPanels(leftPanelObj, rightPanelObj));
        yield return new WaitForSeconds(3);
        newTurnAnnouncer.SetActive(false);
    }

    IEnumerator display_NewTurn(GameObject label) {
        TextMeshProUGUI img = label.GetComponent<TextMeshProUGUI>();
        while (img.color.a < 0.95f) {
            yield return new WaitForSeconds(0.02f);
            img.color = new Color(1, 1, 1, img.color.a + 0.03f);
        }
        img.color = new Color(1, 1, 1, 1);
        yield return new WaitForSeconds(0.5f);
        while (img.color.a > 0.05f) {
            yield return new WaitForSeconds(0.02f);
            img.color = new Color(1, 1, 1, img.color.a - 0.02f);
        }
        img.color = new Color(1, 1, 1, 0);
    }

    IEnumerator display_Name(GameObject nameObj) {
        TextMeshProUGUI img = nameObj.GetComponent<TextMeshProUGUI>();
        while (img.color.a < 0.95f) {
            yield return new WaitForSeconds(0.02f);
            img.color = new Color(1, 1, 1, img.color.a + 0.03f);
        }
        img.color = new Color(1, 1, 1, 1);
        yield return new WaitForSeconds(0.5f);
        while (img.color.a > 0.05f) {
            yield return new WaitForSeconds(0.02f);
            img.color = new Color(1, 1, 1, img.color.a - 0.02f);
        }
        img.color = new Color(1, 1, 1, 0);
    }

    IEnumerator display_LateralPanels(GameObject a, GameObject b) {
        Image imga = a.GetComponent<Image>();
        Image imgb = b.GetComponent<Image>();
        while (imga.color.a < 0.95f) {
            yield return new WaitForSeconds(0.02f);
            imga.color = new Color(1, 1, 1, imga.color.a + 0.06f);
            imgb.color = new Color(1, 1, 1, imgb.color.a + 0.06f);
        }
        imga.color = new Color(1, 1, 1, 1);
        imgb.color = new Color(1, 1, 1, 1);
        yield return new WaitForSeconds(1f);
        while (imga.color.a > 0.05f) {
            yield return new WaitForSeconds(0.02f);
            imga.color = new Color(1, 1, 1, imga.color.a - 0.02f);
            imgb.color = new Color(1, 1, 1, imgb.color.a - 0.02f);
        }
        imga.color = new Color(1, 1, 1, 0);
        imgb.color = new Color(1, 1, 1, 0);
    }

}
