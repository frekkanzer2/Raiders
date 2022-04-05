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

    public void injectPowerUp(Upgrade upgrade, int upLevel) {
        if (SelectionContainer.DUNGEON_MonsterCharactersInfo == null) return;
        if (upLevel == 3) {
            this.hp += upgrade.getHpBonus() * 30 / 100;
            this.bonusHeal += upgrade.getHealBonus() / 3;
            this.actual_hp = this.hp;
            Tuple<int, int, int, int> dmgBonus = upgrade.getAttackBonus();
            this.att_e += dmgBonus.Item1/4;
            this.att_f += dmgBonus.Item2/4;
            this.att_a += dmgBonus.Item3/4;
            this.att_w += dmgBonus.Item4/4;
            Tuple<int, int, int, int> resBonus = upgrade.getDefenceBonus();
            this.res_e += resBonus.Item1/5;
            this.res_f += resBonus.Item2/5;
            this.res_a += resBonus.Item3/5;
            this.res_w += resBonus.Item4/5;
        } else if (upLevel == 2) {
            this.hp += upgrade.getHpBonus() * 45 / 100;
            this.bonusHeal += upgrade.getHealBonus() / 2;
            this.actual_hp = this.hp;
            Tuple<int, int, int, int> dmgBonus = upgrade.getAttackBonus();
            this.att_e += dmgBonus.Item1 / 3;
            this.att_f += dmgBonus.Item2 / 3;
            this.att_a += dmgBonus.Item3 / 3;
            this.att_w += dmgBonus.Item4 / 3;
            Tuple<int, int, int, int> resBonus = upgrade.getDefenceBonus();
            this.res_e += resBonus.Item1 / 4;
            this.res_f += resBonus.Item2 / 4;
            this.res_a += resBonus.Item3 / 4;
            this.res_w += resBonus.Item4 / 4;
        } else if (upLevel == 1) {
            this.hp += upgrade.getHpBonus() * 60 / 100;
            this.bonusHeal += upgrade.getHealBonus();
            this.actual_hp = this.hp;
            Tuple<int, int, int, int> dmgBonus = upgrade.getAttackBonus();
            this.att_e += dmgBonus.Item1 / 2;
            this.att_f += dmgBonus.Item2 / 2;
            this.att_a += dmgBonus.Item3 / 2;
            this.att_w += dmgBonus.Item4 / 2;
            Tuple<int, int, int, int> resBonus = upgrade.getDefenceBonus();
            this.res_e += resBonus.Item1 / 3;
            this.res_f += resBonus.Item2 / 3;
            this.res_a += resBonus.Item3 / 3;
            this.res_w += resBonus.Item4 / 3;
        }
        if (connectedSummoner.heroClass == HeroClass.Steamer) {
            this.actual_shield += upgrade.getShieldBonus() * 20 / 100;
            Tuple<int, int, int, int> dmgBonus = upgrade.getAttackBonus();
            this.att_e += dmgBonus.Item1 / 6;
            this.att_f += dmgBonus.Item2 / 6;
            this.att_a += dmgBonus.Item3 / 6;
            this.att_w += dmgBonus.Item4 / 6;
            Tuple<int, int, int, int> resBonus = upgrade.getDefenceBonus();
            this.res_e += resBonus.Item1 / 6;
            this.res_f += resBonus.Item2 / 6;
            this.res_a += resBonus.Item3 / 6;
            this.res_w += resBonus.Item4 / 6;
        }
        int bonusSummons = upgrade.getSummonsBonus();
        if (bonusSummons == 1) {
            this.hp += this.hp * 20 / 100;
            this.att_e += 20;
            this.att_f += 20;
            this.att_a += 20;
            this.att_w += 20;
            this.actual_hp = this.hp;
            this.bonusHeal += bonusHeal * 20 / 100;
        } else if (bonusSummons == 2) {
            this.hp += this.hp * 40 / 100;
            this.att_e += 50;
            this.att_f += 50;
            this.att_a += 50;
            this.att_w += 50;
            this.pa += 1;
            this.actual_hp = this.hp;
            this.actual_pa = this.pa;
            this.bonusHeal += bonusHeal * 30 / 100;
        }
        if (connectedSummoner.heroClass == HeroClass.Osamodas && bonusSummons > 0) {
            this.hp += ((upgrade.getHpBonus() * 10 / 100) * bonusSummons);
            Tuple<int, int, int, int> dmgBonus = upgrade.getAttackBonus();
            this.att_e += (10 * bonusSummons);
            this.att_f += (10 * bonusSummons);
            this.att_a += (10 * bonusSummons);
            this.att_w += (10 * bonusSummons);
            this.att_e += ((dmgBonus.Item1 / 10) * bonusSummons);
            this.att_f += ((dmgBonus.Item2 / 10) * bonusSummons);
            this.att_a += ((dmgBonus.Item3 / 10) * bonusSummons);
            this.att_w += ((dmgBonus.Item4 / 10) * bonusSummons);
        } else if (connectedSummoner.heroClass == HeroClass.Sadida && bonusSummons > 0) {
            this.hp += ((upgrade.getHpBonus() * 20 / 100) * bonusSummons);
            Tuple<int, int, int, int> resBonus = upgrade.getDefenceBonus();
            this.res_e += ((resBonus.Item1 / 3) * bonusSummons);
            this.res_f += ((resBonus.Item2 / 3) * bonusSummons);
            this.res_a += ((resBonus.Item3 / 3) * bonusSummons);
            this.res_w += ((resBonus.Item4 / 3) * bonusSummons);
        } else if (connectedSummoner.heroClass == HeroClass.Ladrurbo && bonusSummons > 0) {
            Tuple<int, int, int, int> resBonus = upgrade.getDefenceBonus();
            this.res_e += (20 * bonusSummons);
            this.res_f += (20 * bonusSummons);
            this.res_a += (20 * bonusSummons);
            this.res_w += (20 * bonusSummons);
        }
    }

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
                base_damage += 20;
                break;
            case 2:
                base_damage += 42;
                break;
            case 3:
                base_damage += 64;
                break;
            case 4:
                base_damage += 86;
                break;
            case 5:
                base_damage += 112;
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
        base.inflictDamage(damage, mustSkip);
    }

    public override void newTurn() {
        base.newTurn();
        if (this.isBomb && this.bombCharge < 5 && !this.isDead)
            bombCharge++;
        if (this.isBomb && !this.isDead)
            this.receiveHeal(this.hp * 30 / 100);
        if (mustSkip && !this.isDead)
            TurnsManager.Instance.OnSkipTurn();
        if (this.isDouble) {
            doubleCounter++;
            if (doubleCounter == 3)
                this.inflictDamage(this.actual_hp);
        }
    }

}
