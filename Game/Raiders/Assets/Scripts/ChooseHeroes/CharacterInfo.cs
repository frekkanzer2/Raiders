using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterInfo {

    public string characterName;
    public Sprite characterFullSprite;
    public Sprite characterMidSprite;
    public bool isEvocation;

    public bool isEqualsTo(CharacterInfo ci) {
        return this.characterName == ci.characterName;
    }

    public static CharacterInfo generate(string name, Sprite body, Sprite preview, bool isEvocation) {
        CharacterInfo ci = new CharacterInfo();
        ci.characterName = name;
        ci.characterFullSprite = body;
        ci.characterMidSprite = preview;
        ci.isEvocation = isEvocation;
        return ci;
    }

}
