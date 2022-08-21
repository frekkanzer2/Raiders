using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonContainer : MonoBehaviour
{

    public List<DungeonUtils> dungeonsList;
    public bool isDebugEnabled;

    public static int numberOfDungeons = 0;

    private void Start() {
        DungeonContainer.numberOfDungeons = this.dungeonsList.Count;
    }

    private void Update() {
        if (isDebugEnabled)
            if (Input.GetKeyDown(KeyCode.R)) {
                DungeonSave ds = new DungeonSave();
                ds.RESET();
                Debug.LogWarning("Restart the game to apply the reset");
            }
    }

}
