using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class DUNEndGameController : MonoBehaviour
{

    int winningTeam = -1;
    int teamDimension = -1;
    List<string> names = new List<string>();

    public Sprite redVersion;
    public GameObject spriteToChange;

    public GameObject prefab_hero;
    public GameObject whereToGenerate;

    public GameObject manager;

    public GameObject buttonText;

    // Start is called before the first frame update
    void Start()
    {
        winningTeam = PlayerPrefs.GetInt("TEAM_WINNER");
        teamDimension = PlayerPrefs.GetInt("TEAM_DIMENSION");
        if (winningTeam == 1) {
            for (int i = 0; i < teamDimension; i++)
                names.Add(PlayerPrefs.GetString("TEAM_ALPHA_" + i));
            buttonText.GetComponent<TextMeshProUGUI>().text = "Next";
            CharactersLibrary clib = manager.GetComponent<CharactersLibrary>();
            foreach (string name in names) {
                CharacterInfo ci = clib.getCharacterInfoByName(name);
                GameObject generated = Instantiate(prefab_hero);
                generated.transform.SetParent(whereToGenerate.transform);
                generated.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                generated.GetComponent<ChButtonData>().initialize(ci, winningTeam);
                generated.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 1);
            }
        } else {
            buttonText.GetComponent<TextMeshProUGUI>().text = "Back";
            List<CharacterInfo> cilist = SelectionContainer.DUNGEON_MonsterCharactersInfo;
            DungeonSave ds = new DungeonSave();
            int dj_index = ds.getChosenDungeon();
            int dj_room_index = ds.getDungeonRoom();
            List<DungeonUtils> allDungeons = DungeonSave.getAllDungeons();
            DungeonUtils chosenDungeon = allDungeons[dj_index];
            DungeonUtils.RoomMonsters selectedRoom = chosenDungeon.rooms[dj_room_index];
            CharacterInfo info = null;
            foreach(DungeonUtils.RoomTuple rt in selectedRoom.monstersAndQuantity) {
                int monsterID = rt.monsterID;
                DungeonUtils.MonsterPrefab monsterGeneric = chosenDungeon.monsters[monsterID];
                foreach(CharacterInfo ci in cilist)
                    if (ci.characterName.Equals(monsterGeneric.name)) {
                        info = ci;
                        break;
                    }
                GameObject generated = Instantiate(prefab_hero);
                generated.transform.SetParent(whereToGenerate.transform);
                generated.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                generated.GetComponent<ChButtonData>().initialize(info, winningTeam);
                generated.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 1);
            }
        }
    }

    public void OnExitPressed() {
        if (winningTeam == 1) {
            // Next room check or menu check
            DungeonSave ds = new DungeonSave();
            int roomID = ds.getDungeonRoom();
            roomID++;
            if (roomID < 5) {
                Debug.LogWarning("GAMMA CHECKPOINT with room " + roomID);
                ds.saveDungeonRoom(roomID);
                SceneManager.LoadScene("DungeonBattleScene", LoadSceneMode.Single);
            } else {
                if (ds.getReachedDungeonID() < ds.getChosenDungeon())
                    OnDungeonAchieved(ds, ds.getChosenDungeon());
                SceneManager.LoadScene("DungeonChooseCharacters", LoadSceneMode.Single);
            }
        } else {
            SceneManager.LoadScene("DungeonChooseCharacters", LoadSceneMode.Single);
        }
    }

    private void OnDungeonAchieved(DungeonSave dsave, int dungeonID) {
        dsave.dungeonPassed();
    }

}
