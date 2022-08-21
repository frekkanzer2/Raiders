using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class DungeonSave {

    public static string DUNGEON_PROGRESS_STRING = "DUNLASTUNLOCK";
    public static string DUNGEON_CHOSEN_STRING = "DUNGEON_ROOM";
    public static string DUNGEON_ROOM_STRING = "DUNGEON_RNUMBER";
    public static string DUNGEON_BONUS_STRING = "DUNGEON_BONUS";

    public static List<DungeonUtils> BUFFER_ALL_DUNGEONS = null;

    public static void setAllDungeons(List<DungeonUtils> all) {
        BUFFER_ALL_DUNGEONS = all;
    }

    public static List<DungeonUtils> getAllDungeons() {
        return BUFFER_ALL_DUNGEONS;
    }

    public int getReachedDungeonID() {
        if (!PlayerPrefs.HasKey(DUNGEON_PROGRESS_STRING)) return -1;
        return PlayerPrefs.GetInt(DUNGEON_PROGRESS_STRING);
    }

    public int getNextDungeonID(List<DungeonUtils> listOfDungeons) {
        int id = 0;
        if (PlayerPrefs.HasKey(DUNGEON_PROGRESS_STRING)) id = getReachedDungeonID() + 1;
        if (id >= listOfDungeons.Count) id = listOfDungeons.Count - 1;
        return id;
    }

    public string getDungeonNameByID(List<DungeonUtils> listOfDungeons) {
        int id = getReachedDungeonID();
        return listOfDungeons[id].name;
    }

    public Sprite getDungeonSpriteByID(List<DungeonUtils> listOfDungeons) {
        int id = getReachedDungeonID();
        return listOfDungeons[id].bossIcon;
    }

    public List<DungeonUtils.MonsterPrefab> getDungeonMonstersByID(List<DungeonUtils> listOfDungeons) {
        int id = getReachedDungeonID();
        return listOfDungeons[id].monsters;
    }

    public string getDungeonNameByID(List<DungeonUtils> listOfDungeons, int id) {
        return listOfDungeons[id].name;
    }

    public Sprite getDungeonSpriteByID(List<DungeonUtils> listOfDungeons, int id) {
        return listOfDungeons[id].bossIcon;
    }

    public List<DungeonUtils.MonsterPrefab> getDungeonMonstersByID(List<DungeonUtils> listOfDungeons, int id) {
        return listOfDungeons[id].monsters;
    }

    public string getNextDungeonNameByID(List<DungeonUtils> listOfDungeons) {
        int id = getNextDungeonID(listOfDungeons);
        return listOfDungeons[id].name;
    }

    public Sprite getNextDungeonSpriteByID(List<DungeonUtils> listOfDungeons) {
        int id = getNextDungeonID(listOfDungeons);
        return listOfDungeons[id].bossIcon;
    }

    public List<DungeonUtils.MonsterPrefab> getNextDungeonMonstersByID(List<DungeonUtils> listOfDungeons) {
        int id = getNextDungeonID(listOfDungeons);
        return listOfDungeons[id].monsters;
    }

    public void dungeonPassed(int numberOfCharacters, bool isNewDungeon, int dungeonID) {
        if (isNewDungeon) PlayerPrefs.SetInt(DUNGEON_PROGRESS_STRING, getReachedDungeonID() + 1);
        if (numberOfCharacters == 3) {
            if (PlayerPrefs.HasKey(DUNGEON_BONUS_STRING)) {
                string str = PlayerPrefs.GetString(DUNGEON_BONUS_STRING);
                Debug.Log("Picked " + str);
                if (str.Length < DungeonContainer.numberOfDungeons) {
                    for (int i = str.Length-1; i < DungeonContainer.numberOfDungeons; i++) {
                        str += "0";
                    }
                }
                str = EditStringThreeProgress(str, dungeonID);
                PlayerPrefs.SetString(DUNGEON_BONUS_STRING, str);
                Debug.Log("Saved " + str);
            } else {
                string str = "";
                for (int i = 0; i < DungeonContainer.numberOfDungeons; i++)
                    str += "0";
                str = EditStringThreeProgress(str, dungeonID);
                PlayerPrefs.SetString(DUNGEON_BONUS_STRING, str);
                Debug.Log("Saved " + str);
            }
        }
    }

    private string EditStringThreeProgress(string str, int index) {
        StringBuilder sb = new StringBuilder(str);
        sb[index] = '1';
        str = sb.ToString();
        return str;
    }

    public void RESET() {
        PlayerPrefs.SetInt(DUNGEON_PROGRESS_STRING, -1);
    }

    public void saveChosenDungeon(int index) {
        PlayerPrefs.SetInt(DUNGEON_CHOSEN_STRING, index);
    }

    public int getChosenDungeon() {
        return PlayerPrefs.GetInt(DUNGEON_CHOSEN_STRING);
    }

    public void saveDungeonRoom(int room) {
        PlayerPrefs.SetInt(DUNGEON_ROOM_STRING, room);
    }

    public int getDungeonRoom() {
        return PlayerPrefs.GetInt(DUNGEON_ROOM_STRING);
    }

    public int getMapIndex() {
        int room = getDungeonRoom();
        int dungeonid = getChosenDungeon();
        Debug.Log("Room ID: " + room + " | Dungeon ID: " + dungeonid);
        return dungeonid * 5 + room;
    }

}
