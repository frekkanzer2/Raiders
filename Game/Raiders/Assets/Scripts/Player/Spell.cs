using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Spell {

    public enum Element {
        Earth,
        Fire,
        Air,
        Water,
        Heal,
        Other
    }

    public enum DistanceType {
        Normal,
        Line
    }
    
    [HideInInspector]
    public Character link;

    public string name;
    public Sprite icon;
    public Element element; // done
    public int damage; // done
    public bool lifeSteal; // done
    public int minRange; // done
    public int maxRange; // done
    public DistanceType distanceType; // done
    public bool overObstacles; // done
    public int hpCost; // done
    public int paCost; // done
    public int pmCost; // done
    public int maxTimesInTurn;
    public int executeAfterTurns;
    public int effectDuration;
    public float criticalProbability; // done
    public bool hasEffect;
    public bool isEffectOnly;
    public bool canUseInEmptyCell; // done
    public bool isJumpOrEvocation; // done

    public void OnPreviewPressed() {
        Debug.Log("Pressed spell " + name);
        link.displayAttackCells(link, this);
    }

    public static void executeSpell(Character caster, Block targetBlock, Spell spell) {
        if (caster == null) Debug.LogError("CASTER NULL");
        if (targetBlock == null) Debug.LogError("BLOCK NULL");
        if (spell == null) Debug.LogError("SPELL NULL");
        if (!canUse(caster, spell)) return;
        if (targetBlock.linkedObject == null) {
            // no target
            if (spell.canUseInEmptyCell) {
                Debug.Log("Successfully executing " + spell.name);
                payCost(caster, spell);
                // Code here - Spells in empty cells
            } else return;
        } else {
            Debug.Log("Successfully executing " + spell.name);
            payCost(caster, spell);
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            // Code here - Spells on target
            if (spell.damage > 0) {
                int damageToInflict = calculateDamage(caster, target, spell);
                int critProb = UnityEngine.Random.Range(1, 100);
                if (critProb <= spell.criticalProbability) damageToInflict += damageToInflict * 25 / 100;
                Debug.Log("INFLICT " + damageToInflict + " DMGs");
                target.inflictDamage(damageToInflict);
                if (spell.lifeSteal) caster.receiveHeal(damageToInflict / 2);
            }
        }
    }

    public static void payCost(Character caster, Spell spell) {
        caster.actual_hp -= spell.hpCost;
        caster.actual_pa -= spell.paCost;
        caster.actual_pm -= spell.pmCost;
    }

    public static bool canUse(Character caster, Spell spell) {
        if (spell.paCost > caster.actual_pa || spell.pmCost > caster.actual_pm || spell.hpCost > caster.actual_hp) return false;
        return true;
    }

    public static int calculateDamage(Character caster, Character target, Spell spell) {
        Spell.Element element = spell.element;
        int resistance = 0;
        if (element == Spell.Element.Earth) resistance = target.res_e;
        else if (element == Spell.Element.Fire) resistance = target.res_f;
        else if (element == Spell.Element.Air) resistance = target.res_a;
        else if (element == Spell.Element.Water) resistance = target.res_w;
        int bonus_attack = 0;
        if (element == Spell.Element.Earth) bonus_attack = caster.att_e;
        else if (element == Spell.Element.Fire) bonus_attack = caster.att_f;
        else if (element == Spell.Element.Air) bonus_attack = caster.att_a;
        else if (element == Spell.Element.Water) bonus_attack = caster.att_w;
        int damage = spell.damage;
        int finalDamage = (damage + (damage * bonus_attack / 100)) - (damage * resistance / 100);
        return finalDamage;
    }

}
