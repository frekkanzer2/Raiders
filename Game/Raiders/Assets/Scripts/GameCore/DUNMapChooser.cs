using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DUNMapChooser : MonoBehaviour
{

    // Each dungeon has 5 maps!
    public List<TextAsset> mapFiles; // Maps should be set in order!

    public bool isTestingMap = false;
    public TextAsset testingMap;

    // Start is called before the first frame update
    void Start()
    {
        if (!isTestingMap) {
            DungeonSave ds = new DungeonSave();
            TextAsset currentMap = null;
            int chosenIndex = ds.getMapIndex();
            currentMap = mapFiles[chosenIndex];
            GetComponent<MapInitializer>().initialize(currentMap, 1);
        } else GetComponent<MapInitializer>().initialize(testingMap, 1);
    }

}
