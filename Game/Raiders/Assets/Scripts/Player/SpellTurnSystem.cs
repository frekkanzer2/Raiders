using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellTurnSystem : MonoBehaviour {

    public Character connected;
    public List<ParentActiveSpell> activeSpells = new List<ParentActiveSpell>();

    // Start is called before the first frame update
    void Start() {
        connected = GetComponent<Character>();
    }

    public void addEvent(ParentActiveSpell pas) {
        activeSpells.Add(pas);
    }

    public ParentActiveSpell getEvent(string spellName) {
        foreach (ParentActiveSpell pe in activeSpells) {
            if (pe.spellName == spellName)
                return pe;
        }
        return null;
    }

    public void removeZeroSpells() {
        List<ParentActiveSpell> pevs = new List<ParentActiveSpell>();
        foreach (ParentActiveSpell pe in activeSpells) {
            if (pe.turnRemains == 0)
                pevs.Add(pe);
        }
        foreach (ParentActiveSpell pe in pevs)
            activeSpells.Remove(pe);
    }

    public void OnEndTurn() {
        foreach (ParentActiveSpell pe in activeSpells) {
            pe.OnTurnEnds();
        }
        removeZeroSpells();
    }

    public int getNumberOfUses(string name) {
        int counter = 0;
        foreach (ParentActiveSpell pe in activeSpells) {
            if (pe.spellName == name)
                counter++;
        }
        return counter;
    }

    public void removeAllSpells() {
        activeSpells.Clear();
    }

}
