using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Character {
    
    public int id;

    [HideInInspector]
    public bool isCommunionActive = false;

    public override void setDead() {
        base.setDead();
    }

    public string getCompleteName() {
        return this.name + id;
    }

    public override void inflictDamage(int damage, bool mustSkip = false) {
        base.inflictDamage(damage);
    }

    public override void newTurn() {
        base.newTurn();
    }

}
