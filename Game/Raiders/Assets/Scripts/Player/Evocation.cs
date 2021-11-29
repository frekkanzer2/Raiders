using System;
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

    [HideInInspector]
    public bool isBomb = false;
    private Tuple<Character, Spell> bombConnectedInfo;
    private int bombCharge = 0;
    [HideInInspector]
    public bool isTurrect = false;
    [HideInInspector]
    public bool isDouble = false;
    private int doubleCounter = 0;

    public bool mustSkip;

    public void setBomb(Character summoner, Spell attachedSpell) {
        isBomb = true;
        bombConnectedInfo = new Tuple<Character, Spell>(summoner, attachedSpell);
    }

	public void setBombChargeToFive() {
		if (this.isBomb) bombCharge = 5;
	}
	
	public void incrementBombCharge() {
		if (this.isBomb) bombCharge++;
	}

    public int getBombDamage(Character enemy) {
        if (bombConnectedInfo == null) return -1;
        int base_damage = Spell.calculateDamage(bombConnectedInfo.Item1, enemy, bombConnectedInfo.Item2);
        switch (bombCharge) {
            case 1:
                base_damage += 10;
                break;
            case 2:
                base_damage += 20;
                break;
            case 3:
                base_damage += 32;
                break;
            case 4:
                base_damage += 48;
                break;
            case 5:
                base_damage += 76;
                break;
        }
        return base_damage;
    }

    public Sprite getBombSpellSprite() {
        return this.bombConnectedInfo.Item2.icon;
    }

    public override void setDead() {
        connectedSummoner.summons.Remove(this);
        base.setDead();
    }

    public string getCompleteName() {
        return this.name + id;
    }

    public override void inflictDamage(int damage, bool mustSkip = false) {
        if (isCommunionActive) {
            damage /= 2;
            connectedSummoner.inflictDamage(damage, mustSkip);
        }
        base.inflictDamage(damage);
    }

    public override void newTurn() {
        base.newTurn();
        if (this.isBomb && this.bombCharge < 5 && !this.isDead)
            bombCharge++;
        if (mustSkip)
            TurnsManager.Instance.OnSkipTurn();
        if (this.isDouble) {
            doubleCounter++;
            if (doubleCounter == 3)
                this.inflictDamage(this.actual_hp);
        }
    }

}
