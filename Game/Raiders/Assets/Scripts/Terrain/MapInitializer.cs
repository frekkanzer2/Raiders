using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInitializer : MonoBehaviour
{

    public GameObject blockPrefab;
    public List<Sprite> availableSprites;
    public TextAsset mapFile;
    [HideInInspector]
    public Map mapBlocks;

    // Start is called before the first frame update
    void Start()
    {
        mapBlocks = new Map();
        generate();   
    }

    public void generate() {
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
        int row = 0, col = 0; // row and columns
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
                    if (char.IsDigit(_buffer[0])) {
                        GameObject inst = GameObject.Instantiate(blockPrefab);
                        Debug.Log(availableSprites);
                        Debug.Log(int.Parse(_buffer) - 1);
                        inst.GetComponent<Block>().initialize(availableSprites[spriteIds[int.Parse(_buffer)-1]], new Coordinate(row, col));
                        mapBlocks.addBlock(row, col, inst.GetComponent<Block>());
                    }
                    _buffer = "";
                }
                if (c == '\n') {
                    row++;
                    col = 0;
                }
                if (c == '-') col++;
            }
        }
        if (_buffer != "") {
            if (char.IsDigit(_buffer[0])) {
                GameObject inst = GameObject.Instantiate(blockPrefab);
                inst.GetComponent<Block>().initialize(availableSprites[spriteIds[int.Parse(_buffer)-1]], new Coordinate(row, col));
            }
            _buffer = "";
        }
    }

}
