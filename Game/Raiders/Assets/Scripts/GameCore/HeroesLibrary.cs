using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroesLibrary : MonoBehaviour
{

    public List<GameObject> heroesPrefabs_team1;
    public List<GameObject> heroesPrefabs_team2;

    public Character getCharacter(string name, int team) {
        if (team == 1)
            foreach(GameObject chgo in heroesPrefabs_team1) {
                Character ch = chgo.GetComponent<Character>();
                if (ch.name == name) return ch;
            }
        else if (team == 2)
            foreach (GameObject chgo in heroesPrefabs_team2) {
                Character ch = chgo.GetComponent<Character>();
                if (ch.name == name) return ch;
            }
        return null;
    }

    public GameObject getCharacter_GameObject(string name, int team) {
        if (team == 1)
            foreach (GameObject chgo in heroesPrefabs_team1) {
                Character ch = chgo.GetComponent<Character>();
                if (ch.name == name) return chgo;
            } 
        else if (team == 2)
            foreach (GameObject chgo in heroesPrefabs_team2) {
                Character ch = chgo.GetComponent<Character>();
                if (ch.name == name) return chgo;
            }
        return null;
    }

    public CharacterInfo getCharacter_Info(string name) {
        List<CharacterInfo> allInfo = CharactersLibrary.getLibrary();
        foreach (CharacterInfo chi in allInfo)
            if (name == chi.characterName) return chi;
        return null;
    }

}
