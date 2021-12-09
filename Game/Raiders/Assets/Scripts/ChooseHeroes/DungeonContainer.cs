using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonContainer : MonoBehaviour
{

    public List<DungeonUtils> dungeonsList;
    public bool isDebugEnabled;

    private void Update() {
        if (isDebugEnabled)
            if (Input.GetKeyDown(KeyCode.R)) {
                DungeonSave ds = new DungeonSave();
                ds.RESET();
                Debug.LogWarning("Restart the game to apply the reset");
            }
    }

}
