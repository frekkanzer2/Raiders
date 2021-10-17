using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterInfo {

    public string characterName;
    public Sprite characterFullSprite;
    public Sprite characterMidSprite;

    public bool isEqualsTo(CharacterInfo ci) {
        return this.characterName == ci.characterName;
    }

}
