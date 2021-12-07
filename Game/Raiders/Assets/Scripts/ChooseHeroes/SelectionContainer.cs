using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionContainer : MonoBehaviour {

    [HideInInspector]
    public List<CharacterInfo> teamACharacters = new List<CharacterInfo>();
    [HideInInspector]
    public List<CharacterInfo> teamBCharacters = new List<CharacterInfo>();

    [HideInInspector]
    public List<GameObject> teamAHeroes = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> teamBHeroes = new List<GameObject>();

    public static List<CharacterInfo> DUNGEON_MonsterCharactersInfo = null;

    public void removeCharacter(CharacterInfo ci, int team) {
        if (team == 1) {
            CharacterInfo toDelete = null;
            foreach (CharacterInfo chi in teamACharacters) {
                if (chi.isEqualsTo(ci)) {
                    toDelete = chi;
                    break;
                }
            }
            if (toDelete != null) teamACharacters.Remove(toDelete);
        } else if (team == 2) {
            CharacterInfo toDelete = null;
            foreach (CharacterInfo chi in teamBCharacters) {
                if (chi.isEqualsTo(ci)) {
                    toDelete = chi;
                    break;
                }
            }
            if (toDelete != null) teamBCharacters.Remove(toDelete);
        }
    }

    public bool areSameDimension() {
        return teamACharacters.Count == teamBCharacters.Count;
    }

    public void loadSavedTeams(int numberOfTeams) {
        teamACharacters.Clear();
        teamBCharacters.Clear();
        DUNGEON_MonsterCharactersInfo = null;
        if (numberOfTeams == 2) {
            int numberOfHeroes = PlayerPrefs.GetInt("TEAM_DIMENSION");
            GetComponent<CharactersLibrary>().init();
            HeroesLibrary lib = GetComponent<HeroesLibrary>();
            for (int i = 0; i < numberOfHeroes; i++) {
                CharacterInfo ci_temp = lib.getCharacter_Info(PlayerPrefs.GetString("TEAM_ALPHA_" + i));
                teamACharacters.Add(ci_temp);
                ci_temp = lib.getCharacter_Info(PlayerPrefs.GetString("TEAM_BETA_" + i));
                teamBCharacters.Add(ci_temp);
            }
            for (int i = 0; i < numberOfHeroes; i++) {
                GameObject ch_temp = Instantiate(lib.getCharacter_GameObject(teamACharacters[i].characterName, 1));
                ch_temp.transform.position = new Vector3(100000, 100000, 0);
                ch_temp.GetComponent<Character>().team = 1;
                teamAHeroes.Add(ch_temp);
                ch_temp = Instantiate(lib.getCharacter_GameObject(teamBCharacters[i].characterName, 1));
                ch_temp.transform.position = new Vector3(100000, 100000, 0);
                ch_temp.GetComponent<Character>().team = 2;
                teamBHeroes.Add(ch_temp);
            }
            foreach (GameObject go in teamAHeroes) {
                Character c = go.GetComponent<Character>();
            }
            foreach (GameObject go in teamBHeroes) {
                Character c = go.GetComponent<Character>();
            }
        } else if (numberOfTeams == 1) {
            int numberOfHeroes = PlayerPrefs.GetInt("TEAM_DIMENSION");
            GetComponent<CharactersLibrary>().init();
            HeroesLibrary lib = GetComponent<HeroesLibrary>();
            CharacterInfo ci_temp = null;
            for (int i = 0; i < numberOfHeroes; i++) {
                ci_temp = lib.getCharacter_Info(PlayerPrefs.GetString("TEAM_ALPHA_" + i));
                teamACharacters.Add(ci_temp);
            }
            // Here i'm creating instances of heroes
            for (int i = 0; i < numberOfHeroes; i++) {
                GameObject ch_temp = Instantiate(lib.getCharacter_GameObject(teamACharacters[i].characterName, 1));
                ch_temp.transform.position = new Vector3(100000, 100000, 0);
                ch_temp.GetComponent<Character>().team = 1;
                teamAHeroes.Add(ch_temp);
            }
            foreach (GameObject go in teamAHeroes) {
                Character c = go.GetComponent<Character>();
            }
            // Monsters setup
            DungeonSave ds = new DungeonSave();
            int dj_index = ds.getChosenDungeon();
            int dj_room_index = ds.getDungeonRoom();
            List<DungeonUtils> allDungeons = DungeonSave.getAllDungeons();
            DungeonUtils chosenDungeon = allDungeons[dj_index];
            DungeonUtils.RoomMonsters selectedRoom = chosenDungeon.rooms[dj_room_index];
            int monster_id_index = 0;
            SelectionContainer.DUNGEON_MonsterCharactersInfo = new List<CharacterInfo>();
            foreach(DungeonUtils.RoomTuple rt in selectedRoom.monstersAndQuantity) {
                // Generating CharacterInfo for each monster type
                Debug.LogWarning("Must change preview sprite for monster " + chosenDungeon.monsters[rt.monsterID].name);
                CharacterInfo generated = CharacterInfo.generate(chosenDungeon.monsters[rt.monsterID].name, chosenDungeon.monsters[rt.monsterID].sprite, chosenDungeon.monsters[rt.monsterID].sprite, false);
                SelectionContainer.DUNGEON_MonsterCharactersInfo.Add(generated);
                teamBCharacters.Add(generated);
                // Generating all monster instances, based on quantity
                for (int i = 0; i < rt.quantity; i++) {
                    GameObject ch_temp = Instantiate(chosenDungeon.getMonsterPrefab(chosenDungeon.monsters[rt.monsterID].name));
                    ch_temp.transform.position = new Vector3(100000, 100000, 0);
                    ch_temp.GetComponent<Character>().team = 2;
                    ch_temp.GetComponent<Monster>().id = monster_id_index;
                    teamBHeroes.Add(ch_temp);
                    Character c = ch_temp.GetComponent<Character>();
                    monster_id_index++;
                }
            }
            Debug.Log("Generated all monsters successfully");
        }
        
    }

    public List<GameObject> getAll() {
        List<GameObject> toSend = new List<GameObject>();
        toSend.AddRange(teamAHeroes);
        toSend.AddRange(teamBHeroes);
        return toSend;
    }

}
