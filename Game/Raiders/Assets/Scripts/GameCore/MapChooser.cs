using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapChooser : MonoBehaviour
{

    public List<TextAsset> mapFiles;

    public bool isTestingMap = false;
    public TextAsset testingMap;

    // Start is called before the first frame update
    void Start()
    {
        if (!isTestingMap) {
            TextAsset currentMap = null;
            int chosenIndex = Random.Range(0, mapFiles.Count);
            currentMap = mapFiles[chosenIndex];
            GetComponent<MapInitializer>().initialize(currentMap);
        } else GetComponent<MapInitializer>().initialize(testingMap);
    }

}
