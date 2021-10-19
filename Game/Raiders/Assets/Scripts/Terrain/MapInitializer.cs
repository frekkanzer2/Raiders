using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapInitializer : MonoBehaviour
{

    public GameObject blockPrefab;
    public List<Sprite> availableSprites;
    public GameObject mapContainer;
    [HideInInspector]
    public TextAsset mapFile;
    [HideInInspector]
    public Map mapBlocks;

    public GameObject blackScreen;
    public GameObject imageTitle;
    public GameObject mapTitle;
    public bool isDebugEnabled = false;

    private Image img_bs, img_title;
    private TextMeshProUGUI txt_title;

    private void Start() {
        if (!isDebugEnabled) {
            img_bs = blackScreen.GetComponent<Image>();
            img_title = imageTitle.GetComponent<Image>();
            txt_title = mapTitle.GetComponent<TextMeshProUGUI>();
            StartCoroutine(removeBlackScreen());
            StartCoroutine(appearAndRemoveImageTitle());
            StartCoroutine(appearAndRemoveTitle());
        }
    }

    IEnumerator removeBlackScreen() {
        yield return new WaitForSeconds(5);
        while(img_bs.color.a >= 0.001f) {
            yield return new WaitForSeconds(0.02f);
            img_bs.color = new Color(img_bs.color.r, img_bs.color.b, img_bs.color.g, img_bs.color.a - 0.02f);
        }
        img_bs.gameObject.SetActive(false);
        img_bs.gameObject.transform.parent.gameObject.SetActive(false);
        PreparationManager.isPreparationPhaseActived = true;
        GetComponent<PreparationManager>().initializeChooseCards();
    }

    IEnumerator appearAndRemoveImageTitle() {
        yield return new WaitForSeconds(1f);
        while (img_title.color.a <= 0.999f) {
            yield return new WaitForSeconds(0.05f);
            img_title.color = new Color(img_title.color.r, img_title.color.b, img_title.color.g, img_title.color.a + 0.06f);
        }
        yield return new WaitForSeconds(2);
        while (img_title.color.a >= 0.001f) {
            yield return new WaitForSeconds(0.02f);
            img_title.color = new Color(img_title.color.r, img_title.color.b, img_title.color.g, img_title.color.a - 0.03f);
        }
    }

    IEnumerator appearAndRemoveTitle() {
        yield return new WaitForSeconds(1.5f);
        while (txt_title.color.a <= 0.999f) {
            yield return new WaitForSeconds(0.001f);
            txt_title.color = new Color(txt_title.color.r, txt_title.color.b, txt_title.color.g, txt_title.color.a + 0.002f);
        }
        yield return new WaitForSeconds(1f);
        while (txt_title.color.a >= 0.001f) {
            yield return new WaitForSeconds(0.02f);
            txt_title.color = new Color(txt_title.color.r, txt_title.color.b, txt_title.color.g, txt_title.color.a - 0.01f);
        }
        img_title.gameObject.SetActive(false);
    }

    public void initialize(TextAsset chosenMap) {
        mapFile = chosenMap;
        generate();
        if (!isDebugEnabled)
            loadHeroes();
    }

    private void generate() {
        string mapContent = mapFile.text;
        string title = "", ids = "", mapids = "";
        for (int i = 0, j = 0; i < mapContent.Length; i++) {
            if (mapContent[i] != '\n' && j != 2) {
                switch (j) {
                    case 0:
                        title += mapContent[i];
                        break;
                    case 1:
                        ids += mapContent[i];
                        break;
                }
            } else if (j != 2) j++;
            else mapids += mapContent[i];
        }
        if (!isDebugEnabled)
            txt_title.text = title;
        List<int> spriteIds = new List<int>();
        string _buffer = "";
        foreach (char c in ids) {
            if (c!='-' && c != '\n') {
                _buffer += c;
            }
            if (c == '-' || c == '\n') {
                spriteIds.Add(int.Parse(_buffer));
                _buffer = "";
            }
        }
        if (_buffer != "") {
            spriteIds.Add(int.Parse(_buffer));
            _buffer = "";
        }
        _buffer = "";
	    int row = 0, col = 0;
	    int max_col = 0, max_row = 0;
        mapBlocks = new Map();
        // SCANNING MAP
        foreach (char c in mapids) {
            if (c == '0') {
                _buffer = "";
                continue;
            }
            if (c != '-' && c != '\n') {
                _buffer += c;
            }
            if (c == '-' || c == '\n') {
                if (_buffer != "") {
                    GameObject inst = null;
                    if (char.IsDigit(_buffer[0])) {
                        inst = GameObject.Instantiate(blockPrefab);
                        inst.GetComponent<Block>().initialize(availableSprites[spriteIds[int.Parse(""+_buffer[0])-1]], new Coordinate(row, col));
                        mapBlocks.addBlock(row, col, inst.GetComponent<Block>());
                        inst.transform.SetParent(mapContainer.transform);
                    }
                    if (inst != null && _buffer.Length > 1 && !isDebugEnabled) {
                        char teamid = _buffer[1];
                        if (teamid == 'a')
                            inst.GetComponent<Block>().setSpawnable(1);
                        else if (teamid == 'b')
                            inst.GetComponent<Block>().setSpawnable(2);
                    }
                    _buffer = "";
                }
                if (c == '\n') {
	                row++;
	                if (row > max_row) max_row = row;
                    col = 0;
                }
	            if (c == '-') {
	            	col++;
	            	if (col > max_col) max_col = col;
	            }
            }
        }
        if (_buffer != "") {
            GameObject inst = null;
            if (char.IsDigit(_buffer[0])) {
                inst = GameObject.Instantiate(blockPrefab);
                inst.GetComponent<Block>().initialize(availableSprites[spriteIds[int.Parse(""+_buffer[0]) - 1]], new Coordinate(row, col));
                mapBlocks.addBlock(row, col, inst.GetComponent<Block>());
                inst.transform.SetParent(mapContainer.transform);
            }
            if (inst != null && _buffer.Length > 1 && !isDebugEnabled) {
                char teamid = _buffer[1];
                if (teamid == 'a')
                    inst.GetComponent<Block>().setSpawnable(1);
                else if (teamid == 'b')
                    inst.GetComponent<Block>().setSpawnable(2);
            }
	        col++;
	        if (col > max_col) max_col = col;
            _buffer = "";
        }
        float h_toMove = 0, v_toMove = 0;
        Coordinate toMove = new Coordinate(max_row, max_col);
        Vector2 pp = Coordinate.getPosition(toMove);
        h_toMove = pp.x / 2f;
        v_toMove = pp.y / 2f;
        mapBlocks.moveAllBlocksOf(h_toMove, v_toMove);
    }

    private void loadHeroes() {
        SelectionContainer sc = GetComponent<SelectionContainer>();
        sc.loadSavedTeams();
        TMInjector tmi = GetComponent<TMInjector>();
        tmi.InjectIntoTurnsManager();
    }

}
