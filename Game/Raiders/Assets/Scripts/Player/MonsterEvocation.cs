using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterEvocation : Monster
{

    [HideInInspector]
    public Character connectedSummoner;

    public bool mustSkip;

    public override void newTurn() {
        base.newTurn();
        if (mustSkip && !this.isDead)
            TurnsManager.Instance.OnSkipTurn();
    }

}
