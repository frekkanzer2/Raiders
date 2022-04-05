using System;
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


    public bool isBomb = false;
    private Tuple<Character, Spell> bombConnectedInfo;
    public void setBomb(Character summoner, Spell attachedSpell) {
        isBomb = true;
        bombConnectedInfo = new Tuple<Character, Spell>(summoner, attachedSpell);
    }
    public int getBombDamage(Character enemy) {
        if (bombConnectedInfo == null) return -1;
        int base_damage = Spell.calculateDamage(bombConnectedInfo.Item1, enemy, bombConnectedInfo.Item2);
        return base_damage;
    }

}
