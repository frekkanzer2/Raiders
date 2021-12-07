using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DungeonUtils {

    [System.Serializable]
    public class MonsterPrefab {
        public string name;
        public GameObject prefab;
        public Sprite sprite;
    }

    [System.Serializable]
    public class RoomMonsters {
        public string roomNumber;
        public List<RoomTuple> monstersAndQuantity;
    }

    [System.Serializable]
    public class RoomTuple {
        [Header("Monster ID is the index of the monster in the prefab array")]
        public int monsterID;
        public int quantity;
    }

    public string name;
    public Sprite bossIcon;
    public List<MonsterPrefab> monsters;
    public RoomMonsters[] rooms = new RoomMonsters[5];

    public GameObject getMonsterPrefab(string prefabName) {
        foreach (MonsterPrefab mp in this.monsters)
            if (mp.name.Equals(prefabName))
                return mp.prefab;
        return null;
    }

}
