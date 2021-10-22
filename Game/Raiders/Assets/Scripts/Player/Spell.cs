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
	public string description; // done
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
    public bool hasEffect; // done -> SEE THE REGION
    public bool isEffectOnly; // done
    public bool canUseInEmptyCell; // done
    public bool isJumpOrEvocation; // done

    public void OnPreviewPressed() {
        Debug.Log("Pressed spell " + name);
        link.displayAttackCells(link, this);
    }

    public static void payCost(Character caster, Spell spell) {
        caster.actual_hp -= spell.hpCost;
        caster.actual_pa -= spell.paCost;
        caster.actual_pm -= spell.pmCost;
    }

    public static bool canUse(Character caster, Spell spell) {
        if (caster.pm > 0) {
            if (spell.paCost > caster.actual_pa || spell.pmCost > caster.actual_pm || spell.hpCost > caster.actual_hp) return false;
        } else {
            if (spell.paCost > caster.actual_pa || spell.pmCost != 0 || spell.hpCost > caster.actual_hp) return false;
        }
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
        damage += EVENT_BONUS_BASE_DAMAGE(caster, spell);
        int finalDamage = (damage + (damage * bonus_attack / 100)) - (damage * resistance / 100);
        return finalDamage;
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
            if (spell.canUseInEmptyCell)
                return;
            Debug.Log("Successfully executing " + spell.name);
            payCost(caster, spell);
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            // Code here - Spells on target
            if (!spell.isEffectOnly && spell.damage > 0) {
                int damageToInflict = calculateDamage(caster, target, spell);
                int critProb = UnityEngine.Random.Range(1, 101);
                if (critProb <= spell.criticalProbability) damageToInflict += damageToInflict * 25 / 100;
                Debug.Log("INFLICT " + damageToInflict + " DMGs");
                if (spell.element != Element.Heal)
                    target.inflictDamage(damageToInflict);
                else target.receiveHeal(damageToInflict);
                if (spell.lifeSteal) caster.receiveHeal(damageToInflict / 2);
            }
        }
        if (spell.hasEffect) {
            // SPECIALIZATIONS HERE
            SPELL_SPECIALIZATION(caster, targetBlock, spell);
        }
    }

    #region SPELL SPECIALIZATIONS

    public static void SPELL_SPECIALIZATION(Character caster, Block targetBlock, Spell spell) {
        if (spell.name == "Jump" || spell.name == "Portal") EXECUTE_JUMP(caster, targetBlock);
        else if (spell.name == "Pounding") EXECUTE_POUNDING(targetBlock, spell);
        else if (spell.name == "Agitation") EXECUTE_AGITATION(targetBlock, spell);
        else if (spell.name == "Accumulation") EXECUTE_ACCUMULATION(caster, spell);
        else if (spell.name == "Power") EXECUTE_POWER(targetBlock, spell);
        else if (spell.name == "Duel") EXECUTE_DUEL(caster, targetBlock, spell);
        else if (spell.name == "Iop's Wrath") EXECUTE_IOP_WRATH(caster, spell);
        else if (spell.name == "Stretching") EXECUTE_STRETCHING(caster, spell);
        else if (spell.name == "Composure") EXECUTE_COMPOSURE(targetBlock, spell);
        else if (spell.name == "Virus") EXECUTE_VIRUS(caster, spell, targetBlock);
        else if (spell.name == "Powerful Shooting") EXECUTE_POWERFUL_SHOOTING(targetBlock, spell);
        else if (spell.name == "Bow Skill") EXECUTE_BOW_SKILL(caster, spell);
        else if (spell.name == "Slow Down Arrow") EXECUTE_SLOW_DOWN_ARROW(targetBlock, spell);
        else if (spell.name == "Atonement Arrow") EXECUTE_ATONEMENT_ARROW(caster, spell);
        else if (spell.name == "Retreat Arrow") EXECUTE_RETREAT_ARROW(caster, targetBlock);
        // ADD HERE ELSE IF (...) ...
        else Debug.LogError("Effect for " + spell.name + " has not implemented yet");
    }

    public static void EXECUTE_JUMP(Character caster, Block targetBlock) {
        caster.connectedCell.GetComponent<Block>().linkedObject = null;
        caster.connectedCell = targetBlock.gameObject;
        targetBlock.linkedObject = caster.gameObject;
        Vector2 newPosition = Coordinate.getPosition(targetBlock.coordinate);
        caster.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
    }

    public static void EXECUTE_POUNDING(Block targetBlock, Spell s) {
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 30) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new PoundingEvent("Pounding", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_AGITATION(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        AgitationEvent agitationEvent = new AgitationEvent("Agitation", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
        target.addEvent(agitationEvent);
        if (target.name == s.link.name && target.team == s.link.team) {
            agitationEvent.useIstantanely();
        }
    }

    public static void EXECUTE_ACCUMULATION(Character caster, Spell s) {
        caster.addEvent(new AccumulationEvent("Accumulation", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_POWER(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        PowerEvent powerEvent = new PowerEvent("Power", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(powerEvent);
        if (target.name == s.link.name && target.team == s.link.team)
            powerEvent.useIstantanely();
    }

    public static void EXECUTE_DUEL(Character caster, Block targetBlock, Spell s) {
        DuelEvent casterEvent = new DuelEvent("Duel", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
        casterEvent.useIstantanely();
        caster.addEvent(casterEvent);
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new DuelEvent("Duel", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_IOP_WRATH(Character caster, Spell s) {
        caster.addEvent(new IopWrathEvent("Iop's Wrath", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_STRETCHING(Character caster, Spell s) {
        StretchingEvent se = new StretchingEvent("Stretching", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        se.useIstantanely();
        caster.addEvent(se);
    }

    public static void EXECUTE_COMPOSURE(Block targetBlock, Spell s) {
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 50) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new ComposureEvent("Composure", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
        }
    }

    public static void EXECUTE_VIRUS(Character caster, Spell s, Block targetBlock) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character ch in TurnsManager.Instance.turns) {
            if (ch.team == caster.team && ch.name != caster.name) {
                ch.receiveHeal(calculateDamage(caster, target, s) / 2);
            }
        }
    }

    public static void EXECUTE_POWERFUL_SHOOTING(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        PowerfulShooting powerEvent = new PowerfulShooting("Powerful Shooting", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(powerEvent);
        if (target.name == s.link.name && target.team == s.link.team)
            powerEvent.useIstantanely();
    }

    public static void EXECUTE_BOW_SKILL(Character caster, Spell s) {
        caster.addEvent(new BowSkill("Bow Skill", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_SLOW_DOWN_ARROW(Block targetBlock, Spell s) {
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 60) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new SlowDownArrowEvent("Slow Down Arrow", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_ATONEMENT_ARROW(Character caster, Spell s) {
        caster.addEvent(new AtonementArrowEvent("Atonement Arrow", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_RETREAT_ARROW(Character caster, Block targetBlock) {
        int numberOfCellsToMove = 3;
        List<Block> path = new List<Block>();
        Coordinate casterPosition = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetPosition = targetBlock.coordinate;
        Debug.Log("Coordinate hit: " + targetPosition.display());
        if (targetPosition.row > casterPosition.row) {
            Debug.Log("Target is down");
            // target is down
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row + i, targetPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.row < casterPosition.row) {
            Debug.Log("Target is up");
            // target is up
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row - i, targetPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column > casterPosition.column) {
            Debug.Log("Target is on the right");
            // target is on the right
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row, targetPosition.column + i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column < casterPosition.column) {
            Debug.Log("Target is on the left");
            // target is on the left
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row, targetPosition.column - i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else {
            Debug.LogError("RETREAT ARROW error case");
        }
        if (path.Count > 0) {
            Debug.LogWarning("*** PATH ***");
            foreach(Block b in path) {
                Debug.LogWarning(b.coordinate.display());
            }
            targetBlock.linkedObject.GetComponent<Character>().setPath(path); // move the enemy
        }
    }

    #endregion

    #region EVENT BONUSES

    public static int BONUS_ACCUMULATION = 4;
    public static int BONUS_WRATH = 120;
    public static int BONUS_BOW_SKILL = 12;
    public static int BONUS_ATONEMENT_ARROW = 36;

    public static int EVENT_BONUS_BASE_DAMAGE(Character caster, Spell s) {
        if (caster.name == "Missiz Frizz" && s.name == "Accumulation") {
            List<ParentEvent> acclist = caster.getEventSystem().getEvents("Accumulation");
            return BONUS_ACCUMULATION * acclist.Count;
        } else if (caster.name == "Ragedala" && s.name == "Iop's Wrath") {
            List<ParentEvent> acclist = caster.getEventSystem().getEvents("Iop's Wrath");
            return BONUS_WRATH * acclist.Count;
        } else if (caster.name == "Voldorak" && s.name == "Bow Skill") {
            List<ParentEvent> bslist = caster.getEventSystem().getEvents("Bow Skill");
            return BONUS_BOW_SKILL * bslist.Count;
        } else if (caster.name == "Arc Piven" && s.name == "Atonement Arrow") {
            List<ParentEvent> bslist = caster.getEventSystem().getEvents("Atonement Arrow");
            return BONUS_ATONEMENT_ARROW * bslist.Count;
        } else return 0;
    }

    #endregion

}
