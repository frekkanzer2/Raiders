using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evocation : Character {
    
    [HideInInspector]
    public Character connectedSummoner;

    [HideInInspector]
    public int id;

    [HideInInspector]
    public bool isCommunionActive = false;

    public bool mustSkip;

    public override void setDead() {
        connectedSummoner.summons.Remove(this);
        base.setDead();
    }

    public string getCompleteName() {
        return this.name + id;
    }

    public override void inflictDamage(int damage) {
        if (isCommunionActive) {
            damage /= 2;
            connectedSummoner.inflictDamage(damage);
        }
        base.inflictDamage(damage);
    }

    public override void newTurn() {
        base.newTurn();
        if (mustSkip)
            TurnsManager.Instance.OnSkipTurn();
    }

}
