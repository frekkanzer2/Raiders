using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSave {

    public string DUNGEON_PROGRESS_STRING = "DUNLASTUNLOCK";
    public string DUNGEON_CHOSEN_STRING = "DUNGEON_ROOM";
    public string DUNGEON_ROOM_STRING = "DUNGEON_RNUMBER";

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

    public void dungeonPassed() {
        PlayerPrefs.SetInt(DUNGEON_PROGRESS_STRING, getReachedDungeonID() + 1);
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
