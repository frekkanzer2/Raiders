using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroesLibrary : MonoBehaviour
{

    public List<GameObject> heroesPrefabs;

    public Character getCharacter(string name) {
        foreach(GameObject chgo in heroesPrefabs) {
            Character ch = chgo.GetComponent<Character>();
            if (ch.name == name) return ch;
        }
        return null;
    }

    public GameObject getCharacter_GameObject(string name) {
        foreach (GameObject chgo in heroesPrefabs) {
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
