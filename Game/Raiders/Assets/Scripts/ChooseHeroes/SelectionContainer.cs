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

    public void loadSavedTeams() {
        int numberOfHeroes = PlayerPrefs.GetInt("TEAM_DIMENSION");
        teamACharacters.Clear(); teamBCharacters.Clear();
        GetComponent<CharactersLibrary>().init();
        HeroesLibrary lib = GetComponent<HeroesLibrary>();
        for (int i = 0; i < numberOfHeroes; i++) {
            CharacterInfo ci_temp = lib.getCharacter_Info(PlayerPrefs.GetString("TEAM_ALPHA_" + i));
            if (ci_temp == null) Debug.LogError("Failed for chInfo " + PlayerPrefs.GetString("TEAM_ALPHA_" + i));
            teamACharacters.Add(ci_temp);
            ci_temp = lib.getCharacter_Info(PlayerPrefs.GetString("TEAM_BETA_" + i));
            if (ci_temp == null) Debug.LogError("Failed for chInfo " + PlayerPrefs.GetString("TEAM_BETA_" + i));
            teamBCharacters.Add(ci_temp);
        }
        for (int i = 0; i < numberOfHeroes; i++) {
            GameObject ch_temp = lib.getCharacter_GameObject(teamACharacters[i].characterName);
            if (ch_temp == null) Debug.LogError("Failed for hero " + teamACharacters[i].characterName);
            ch_temp.GetComponent<Character>().team = 1;
            teamAHeroes.Add(ch_temp);
            ch_temp = lib.getCharacter_GameObject(teamBCharacters[i].characterName);
            if (ch_temp == null) Debug.LogError("Failed for hero " + teamACharacters[i].characterName);
            ch_temp.GetComponent<Character>().team = 2;
            teamBHeroes.Add(ch_temp);
        }
    }

    public List<GameObject> getAll() {
        List<GameObject> toSend = new List<GameObject>();
        toSend.AddRange(teamAHeroes);
        toSend.AddRange(teamBHeroes);
        return toSend;
    }

    public GameObject getHeroFromTeam(Character c) {
        if (c.team == 1)
            foreach(GameObject go in teamAHeroes)
                if (go.GetComponent<Character>().name == c.name)
                    return go;
        if (c.team == 2)
            foreach (GameObject go in teamBHeroes)
                if (go.GetComponent<Character>().name == c.name)
                    return go;
        return null;
    }

}
