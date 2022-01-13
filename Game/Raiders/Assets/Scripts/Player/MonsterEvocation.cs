using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterEvocation : Monster
{

    [HideInInspector]
    public Character connectedSummoner;

    public bool mustSkip;

    public override string getCompleteName() {
        return ((Monster)this.connectedSummoner).getCompleteName() + "-" + this.name + id;
    }

    public override void newTurn() {
        base.newTurn();
        if (mustSkip && !this.isDead)
            TurnsManager.Instance.OnSkipTurn();
    }

}
