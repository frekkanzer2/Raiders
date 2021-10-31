using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentActiveSpell {

    public string spellName;
    public int turnRemains;
    public Spell reference;
    
    public ParentActiveSpell(Spell reference) {
        this.spellName = reference.name;
        this.turnRemains = reference.executeAfterTurns;
        this.reference = reference;
    }

    public void OnTurnEnds() {
        execute();
    }
    
    public void execute() {
        if (this.turnRemains > 0)
            this.turnRemains--;
    }

}
