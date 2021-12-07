using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreparationManager : MonoBehaviour
{

    private static PreparationManager _instance;
    public static PreparationManager Instance { get { return _instance; } }

    public static bool isPreparationPhaseActived = false; // Activated by map initializer
    public static bool isChoosingPlayer = false;

    public GameObject panelChooseAlpha;
    public GameObject panelChooseBeta;

    public GameObject contentAlpha;
    public GameObject contentBeta;
    public GameObject cellContentAlphaPrefab;
    public GameObject cellContentBetaPrefab;

    private bool isAlphaReady = false;
    private bool isBetaReady = false;

    public Sprite blueButton;
    public Sprite redButton;
    public Sprite yellowButton;
    public GameObject btnAlpha;
    public GameObject btnBeta;
    public GameObject prefab_BlueStand;
    public GameObject prefab_RedStand;

    // int => team; Character => connected generated character; CharacterInfo => connected info; CellHeroChooser => connected cell chooser button; GameObject => hero gameobject
    private List<Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject>> registeredCells = new List<Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject>>();

    [HideInInspector]
    public static Block consideredBlock = null;

    public void deactivateReadyButtons() {
        if (!isPreparationPhaseActived) return;
        btnAlpha.GetComponent<Image>().sprite = blueButton;
        btnBeta.GetComponent<Image>().sprite = redButton;
        btnAlpha.SetActive(false);
        btnBeta.SetActive(false);
        isAlphaReady = false;
        isBetaReady = false;
    }

    public void activateReadyButtons() {
        if (!isPreparationPhaseActived) return;
        btnAlpha.GetComponent<Image>().sprite = blueButton;
        btnBeta.GetComponent<Image>().sprite = redButton;
        btnAlpha.SetActive(true);
        btnBeta.SetActive(true);
        isAlphaReady = false;
        isBetaReady = false;
    }

    public void OnAlphaReady() {
        if (registeredCells.Count != PlayerPrefs.GetInt("TEAM_DIMENSION")*2) return;
        if (!isAlphaReady && isPreparationPhaseActived) {
            isAlphaReady = true;
            btnAlpha.GetComponent<Image>().sprite = yellowButton;
            if (isBetaReady) SoundUi.Instance.playAudio(SoundUi.AudioType.HeroChoise_Confirm2);
            else SoundUi.Instance.playAudio(SoundUi.AudioType.HeroChoise_Confirm1);
        } else if (isPreparationPhaseActived) {
            isAlphaReady = false;
            btnAlpha.GetComponent<Image>().sprite = blueButton;
        }
    }

    public void OnBetaReady() {
        if (registeredCells.Count != PlayerPrefs.GetInt("TEAM_DIMENSION")*2) return;
        if (!isBetaReady && isPreparationPhaseActived) {
            isBetaReady = true;
            btnBeta.GetComponent<Image>().sprite = yellowButton;
            if (isAlphaReady) SoundUi.Instance.playAudio(SoundUi.AudioType.HeroChoise_Confirm2);
            else SoundUi.Instance.playAudio(SoundUi.AudioType.HeroChoise_Confirm1);
        } else if (isPreparationPhaseActived) {
            isBetaReady = false;
            btnBeta.GetComponent<Image>().sprite = redButton;
        }
    }

    public void initializeChooseCards() {
        TurnsManager tm = GetComponent<TurnsManager>();
        List<Character> charsInTurn = tm.turns;
        CharactersLibrary cl = GetComponent<CharactersLibrary>();
        // Not bot case
        if (SelectionContainer.DUNGEON_MonsterCharactersInfo == null)
            foreach (Character c in charsInTurn) {
                CharacterInfo retrievedInfo = cl.getCharacterInfoByName(c.name);
                if (c.team == 1) {
                    GameObject spawned = Instantiate(cellContentAlphaPrefab);
                    spawned.transform.SetParent(contentAlpha.transform);
                    spawned.GetComponent<CellHeroChooser>().initialize(retrievedInfo, this, 1);
                    spawned.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                } else if (c.team == 2) {
                    GameObject spawned = Instantiate(cellContentBetaPrefab);
                    spawned.transform.SetParent(contentBeta.transform);
                    spawned.GetComponent<CellHeroChooser>().initialize(retrievedInfo, this, 2);
                    spawned.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                }
            }
        else { // Bot case
            foreach (Character c in charsInTurn) {
                if (c.team == 1) {
                    CharacterInfo retrievedInfo = cl.getCharacterInfoByName(c.name);
                    GameObject spawned = Instantiate(cellContentAlphaPrefab);
                    spawned.transform.SetParent(contentAlpha.transform);
                    spawned.GetComponent<CellHeroChooser>().initialize(retrievedInfo, this, 1);
                    spawned.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
                }
            }
            Debug.LogError("NOT IMPLEMENTED FOR DUNGEON VERSION - CONTINUE HERE!");
            // Enemies must spawn on the map - set their spawn cells!
        }
    }

    public void OnCellChoosePress(int team, CharacterInfo ci, CellHeroChooser chc) {
        if (!isPreparationPhaseActived) return;
        TurnsManager tm = GetComponent<TurnsManager>();
        Character ch = tm.getCharacterInTurns(ci.characterName, team);
        GameObject ch_go = ch.gameObject;
        Debug.LogWarning("CONSIDERED BLOCK: " + consideredBlock.coordinate.display());
        ch_go.transform.position = Coordinate.getPosition(consideredBlock.coordinate);
        ch.connectedCell = consideredBlock.gameObject;
        consideredBlock.linkedObject = ch_go;
        ch.setZIndex(ch.connectedCell.GetComponent<Block>());
        registeredCells.Add(new Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject>(team, ch, ci, chc, ch_go));
        if (registeredCells.Count == PlayerPrefs.GetInt("TEAM_DIMENSION")*2)
            activateReadyButtons();
        tm.addRelation(ch_go, ci);
        Debug.Log("PRECALL");
        SoundUi.Instance.playAudio(SoundUi.AudioType.HeroChoise_SetHeroInCell);
        closeChooseScreen();
    }

    public void OnCellAlreadyChosen(Character pressed) {
        if (!isPreparationPhaseActived) return;
        deactivateReadyButtons();
        Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject> toDel = null;
        foreach (Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject> t in registeredCells) {
            if (t.Item2.name == pressed.name && t.Item2.team == pressed.team) {
                toDel = t;
                break;
            }
        }
        if (toDel == null) return;
        registeredCells.Remove(toDel);
        pressed.connectedCell.GetComponent<Block>().linkedObject = null;
        pressed.connectedCell = null;
        TurnsManager tm = GetComponent<TurnsManager>();
        tm.removeRelation(pressed);
        pressed.gameObject.transform.position = new Vector3(100000, 100000, 0);
        toDel.Item4.gameObject.SetActive(true);
    }

    IEnumerator goToNextPhase() {
        yield return new WaitForSeconds(0.2f);
        List<Block> bs = Map.Instance.getAllBlocks();
        foreach(Block b in bs) {
            b.resetColor();
        }
        btnAlpha.SetActive(false);
        btnBeta.SetActive(false);
        yield return new WaitForSeconds(1);
        TurnsManager tm = GetComponent<TurnsManager>();
        // ADD HERE STANDS
        foreach(Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject> t in registeredCells) {
            GameObject heroGo = t.Item5;
            int team = t.Item1;
            GameObject stand = null;
            if (team == 1)
                stand = Instantiate(prefab_BlueStand);
            else
                stand = Instantiate(prefab_RedStand);
            stand.transform.SetParent(heroGo.transform);
        }
        tm.OnStartGame();
    }

    public void setStandManually(GameObject hero, int team) {
        GameObject stand = null;
        if (team == 1)
            stand = Instantiate(prefab_BlueStand);
        else
            stand = Instantiate(prefab_RedStand);
        stand.transform.SetParent(hero.transform);
    }

    private void Start() {
        if (PreparationManager.Instance == null) PreparationManager._instance = this;
    }

    IEnumerator openPanelWithDelay(Block selected) {
        yield return new WaitForSeconds(0.2f);
        int t = selected.getSpawnableTeam();
        if (t == 1)
            panelChooseAlpha.SetActive(true);
        else if (t == 2)
            panelChooseBeta.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPreparationPhaseActived && !isChoosingPlayer)
            if (Input.GetMouseButtonDown(0)) {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                if (hit.collider != null) {
                    if (hit.collider.gameObject.CompareTag("Block")) {
                        Block selected = hit.collider.gameObject.GetComponent<Block>();
                        consideredBlock = selected;
                        if (selected.canSpawnHero() && selected.linkedObject == null) {
                            CameraDragDrop.canMove = false;
                            isChoosingPlayer = true;
                            StartCoroutine(openPanelWithDelay(selected));
                        } else if (selected.canSpawnHero() && selected.linkedObject != null) {
                            PreparationManager.Instance.OnCellAlreadyChosen(selected.linkedObject.GetComponent<Character>());
                        }
                    }
                }
            }
        if (isAlphaReady && isBetaReady && isPreparationPhaseActived) {
            isPreparationPhaseActived = false;
            SoundMusic.Instance.play(SoundMusic.Instance.getSoundtrack(PlayerPrefs.GetString("CHOSEN_MAP")));
            StartCoroutine(goToNextPhase());
        }
    }

    public void closeChooseScreen() {
        Debug.LogWarning("Closed screen here");
        panelChooseAlpha.SetActive(false);
        panelChooseBeta.SetActive(false);
        CameraDragDrop.canMove = true;
        isChoosingPlayer = false;
        consideredBlock = null;
    }

}
