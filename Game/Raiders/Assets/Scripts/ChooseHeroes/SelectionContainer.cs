using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionContainer : MonoBehaviour {

    public List<CharacterInfo> teamACharacters = new List<CharacterInfo>();
    public List<CharacterInfo> teamBCharacters = new List<CharacterInfo>();

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

}
