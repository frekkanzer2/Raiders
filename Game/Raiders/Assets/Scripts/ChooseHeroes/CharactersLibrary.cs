using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersLibrary : MonoBehaviour
{

    public List<CharacterInfo> library;
    public static List<CharacterInfo> public_library;

    private void Start() {
        CharactersLibrary.public_library = this.library;
    }

    public void init() {
        CharactersLibrary.public_library = this.library;
    }

    public static List<CharacterInfo> getLibrary() {
        return CharactersLibrary.public_library;
    }

    public CharacterInfo getCharacterInfoByName(string n) {
        foreach(CharacterInfo ci in library) {
            if (ci.characterName == n) return ci;
        }
        return null;
    }

    public CharacterInfo getCharacterInfoMonster(Monster m) {
        List<CharacterInfo> cim = SelectionContainer.DUNGEON_MonsterCharactersInfo;
        foreach (CharacterInfo ci in cim)
            if (ci.characterName.Equals(m.name))
                return ci;
        return null;
    }

    public static int getNumberOfEvocations() {
        int counter = 0;
        foreach (CharacterInfo ci in public_library) if (ci.isEvocation) counter++;
        return counter;
    }

}
