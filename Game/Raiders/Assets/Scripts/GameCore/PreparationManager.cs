using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // int => team; Character => connected generated character; CharacterInfo => connected info; CellHeroChooser => connected cell chooser button
    private List<Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject>> registeredCells = new List<Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject>>();

    [HideInInspector]
    public static Block consideredBlock = null;
    
    public void initializeChooseCards() {
        TurnsManager tm = GetComponent<TurnsManager>();
        List<Character> charsInTurn = tm.turns;
        CharactersLibrary cl = GetComponent<CharactersLibrary>();
        foreach (Character c in charsInTurn) {
            CharacterInfo retrievedInfo = cl.getCharacterInfoByName(c.name);
            if (c.team == 1) {
                GameObject spawned = Instantiate(cellContentAlphaPrefab);
                spawned.transform.SetParent(contentAlpha.transform);
                spawned.GetComponent<CellHeroChooser>().initialize(retrievedInfo, this, 1);
            } else if (c.team == 2) {
                GameObject spawned = Instantiate(cellContentBetaPrefab);
                spawned.transform.SetParent(contentBeta.transform);
                spawned.GetComponent<CellHeroChooser>().initialize(retrievedInfo, this, 2);
            }
        }
    }

    public void OnCellChoosePress(int team, CharacterInfo ci, CellHeroChooser chc) {
        TurnsManager tm = GetComponent<TurnsManager>();
        Character ch = tm.getCharacterInTurns(ci.characterName, team);
        GameObject ch_go = ch.gameObject;
        ch_go.transform.position = Coordinate.getPosition(consideredBlock.coordinate);
        ch.connectedCell = consideredBlock.gameObject;
        consideredBlock.linkedObject = ch_go;
        ch.setZIndex(ch.connectedCell.GetComponent<Block>());
        registeredCells.Add(new Tuple<int, Character, CharacterInfo, CellHeroChooser, GameObject>(team, ch, ci, chc, ch_go));
        tm.addRelation(ch_go, ci);
        closeChooseScreen();
    }

    public void OnCellAlreadyChosen(Character pressed) {
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

    private void Start() {
        if (PreparationManager.Instance == null) PreparationManager._instance = this;
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
                            int t = selected.getSpawnableTeam();
                            CameraDragDrop.canMove = false;
                            isChoosingPlayer = true;
                            if (t == 1)
                                panelChooseAlpha.SetActive(true);
                            else if (t == 2)
                                panelChooseBeta.SetActive(true);
                        } else if (selected.canSpawnHero() && selected.linkedObject != null) {
                            PreparationManager.Instance.OnCellAlreadyChosen(selected.linkedObject.GetComponent<Character>());
                        }
                    }
                }
            }
    }

    public void closeChooseScreen() {
        panelChooseAlpha.SetActive(false);
        panelChooseBeta.SetActive(false);
        CameraDragDrop.canMove = true;
        isChoosingPlayer = false;
        consideredBlock = null;
    }

}
