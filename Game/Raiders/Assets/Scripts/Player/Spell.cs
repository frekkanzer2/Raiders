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
        caster.decrementHP_withoutEffect(spell.hpCost);
        caster.decrementPA_withoutEffect(spell.paCost);
        caster.decrementPM_withoutEffect(spell.pmCost);
    }

    public static bool canUse(Character caster, Spell spell) {
        // Controllo esecuzione incantesimo dopo tot turni
        if (spell.executeAfterTurns > 0 && caster.getSpellSystem().getNumberOfUses(spell.name) == 1) return false;
        // Controllo numero di esecuzioni in un turno
        if (spell.maxTimesInTurn > 0 && spell.maxTimesInTurn == caster.getSpellSystem().getNumberOfUses(spell.name)) return false;
        // Controllo sulla durata dell'incantesimo
        //if (spell.effectDuration < -1) return false;
        if (caster.getActualPM() < 0) {
            if (spell.paCost > caster.getActualPA() || spell.pmCost != 0 || spell.hpCost > caster.getActualHP()) return false;
        } else {
            if (spell.paCost > caster.getActualPA() || spell.pmCost > caster.getActualPM() || spell.hpCost > caster.getActualHP()) return false;
        }
        return true;
    }
    
    public static int getRemainingTurns(Character caster, Spell spell) {
        if (spell.executeAfterTurns > 0 && caster.getSpellSystem().getNumberOfUses(spell.name) == 1)
            return caster.getSpellSystem().getEvent(spell.name).turnRemains;
        else return -1; // return
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
        damage += EVENT_BONUS_BASE_DAMAGE(caster, target, spell);
        int finalDamage = (damage + (damage * bonus_attack / 100)) - (damage * resistance / 100);
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int critProb = UnityEngine.Random.Range(1, 101);
        if (caster.canCritical) {
            if (!caster.criticShooting) {
                if (critProb <= spell.criticalProbability)
                    finalDamage += finalDamage * 25 / 100;
            } else {
                if (critProb <= spell.criticalProbability + 14)
                    finalDamage += finalDamage * 25 / 100;
            }
        }
        if (ut_isNearOf(caster, target, 3) && target.immuneCloseCombat)
            finalDamage = 0;
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
                Debug.Log("INFLICT " + damageToInflict + " DMGs");
                if (spell.element != Element.Heal) {
                    if (target.connectedSacrifice == null)
                        target.inflictDamage(damageToInflict);
                    else target.connectedSacrifice.inflictDamage(damageToInflict);
                } else target.receiveHeal(damageToInflict);
                if (spell.lifeSteal) caster.receiveHeal(damageToInflict / 2);
            }
        }
        caster.addSpell(spell);
        if (spell.hasEffect) {
            // SPECIALIZATIONS HERE
            SPELL_SPECIALIZATION(caster, targetBlock, spell);
        }
    }

    #region SPELL SPECIALIZATIONS

    public static void SPELL_SPECIALIZATION(Character caster, Block targetBlock, Spell spell) {
        if (spell.name == "Jump" || spell.name == "Portal" || spell.name == "Catnip") EXECUTE_JUMP(caster, targetBlock);
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
        else if (spell.name == "Barricade Shot") EXECUTE_BARRICADE_SHOT(caster, targetBlock, spell);
        else if (spell.name == "Sentinel") EXECUTE_SENTINEL(caster, spell);
        else if (spell.name == "Critical Shooting") EXECUTE_CRITICAL_SHOOTING(targetBlock, spell);
        else if (spell.name == "Exodus" || spell.name == "Feline Spirit") EXECUTE_EXODUS(caster, targetBlock, spell);
        else if (spell.name == "Convulsion") EXECUTE_CONVULSION(caster, targetBlock);
        else if (spell.name == "Therapy") EXECUTE_THERAPY(caster, targetBlock);
        else if (spell.name == "Odyssey") EXECUTE_ODYSSEY(caster);
        else if (spell.name == "Transposition" || spell.name == "Assault" || spell.name == "Lifting Word") EXECUTE_TRANSPOSITION(caster, targetBlock);
        else if (spell.name == "Attraction") EXECUTE_ATTRACTION(caster, targetBlock);
        else if (spell.name == "Desolation") EXECUTE_DESOLATION(targetBlock, spell);
        else if (spell.name == "Mutilation") EXECUTE_MUTILATION(caster, spell);
        else if (spell.name == "Berserk") EXECUTE_BERSERK(caster, spell);
        else if (spell.name == "Influx") EXECUTE_INFLUX(caster, targetBlock);
        else if (spell.name == "Sacrifice") EXECUTE_SACRIFICE(caster, targetBlock, spell);
        else if (spell.name == "Transfusion") EXECUTE_TRANSFUSION(caster, spell);
        else if (spell.name == "Smell") EXECUTE_SMELL(targetBlock, spell);
        else if (spell.name == "Heads or Tails") EXECUTE_HEADS_OR_TAILS(targetBlock, spell);
        else if (spell.name == "All or Nothing") EXECUTE_ALL_OR_NOTHING(targetBlock, spell);
        else if (spell.name == "Claw of Ceangal") EXECUTE_CLAW_OF_CEANGAL(caster, targetBlock);
        else if (spell.name == "Godsend") EXECUTE_GODSEND(targetBlock, spell);
        else if (spell.name == "Feline Sense") EXECUTE_FELINE_SENSE(targetBlock, spell);
        else if (spell.name == "Roulette") EXECUTE_ROULETTE(caster, spell);
        else if (spell.name == "Time Rift") EXECUTE_TIME_RIFT(caster, targetBlock, spell);
        else if (spell.name == "Sandglass") EXECUTE_SANDGLASS(targetBlock, spell);
        else if (spell.name == "Rewind") EXECUTE_REWIND(targetBlock, spell);
        else if (spell.name == "Clock") EXECUTE_CLOCK(targetBlock, spell);
        else if (spell.name == "Time Theft") EXECUTE_TIME_THEFT(caster, targetBlock, spell);
        else if (spell.name == "Haziness") EXECUTE_HAZINESS(targetBlock, spell);
        else if (spell.name == "Slow Down") EXECUTE_SLOW_DOWN(targetBlock, spell);
        else if (spell.name == "Gear") EXECUTE_GEAR(targetBlock, spell);
        else if (spell.name == "Restart") EXECUTE_RESTART(spell);
        else if (spell.name == "Stampede") EXECUTE_STAMPEDE(caster, targetBlock, spell);
        else if (spell.name == "Capering") EXECUTE_CAPERING(caster, targetBlock, spell);
        else if (spell.name == "Coward Mask") SWITCH_COWARD_MASK(caster, spell);
        else if (spell.name == "Psychopath Mask") SWITCH_PSYCHOPATH_MASK(caster, spell);
        else if (spell.name == "Tireless Mask") SWITCH_TIRELESS_MASK(caster, spell);
        else if (spell.name == "Tortuga") EXECUTE_TORTUGA(targetBlock, spell);
        else if (spell.name == "Apathy") EXECUTE_APATHY(targetBlock, spell);
        else if (spell.name == "Furia") EXECUTE_FURIA(caster, targetBlock);
        else if (spell.name == "Comedy") EXECUTE_COMEDY(caster, targetBlock);
        else if (spell.name == "Apostasy") EXECUTE_APOSTASY(caster, targetBlock);
        else if (spell.name == "Lightness") EXECUTE_LIGHTNESS(caster, spell);
        else if (spell.name == "Puddle Glyph") EXECUTE_PUDDLE_GLYPH(caster, spell);
        else if (spell.name == "Aggressive Glyph") EXECUTE_AGGRESSIVE_GLYPH(caster, spell);
        else if (spell.name == "Protective Glyph") EXECUTE_PROTECTIVE_GLYPH(caster, spell);
        else if (spell.name == "Perception Glyph") EXECUTE_PERCEPTION_GLYPH(caster, spell);
        else if (spell.name == "Barricade") EXECUTE_BARRICADE(targetBlock, spell);
        else if (spell.name == "Fortification") EXECUTE_FORTIFICATION(targetBlock, spell);
        else if (spell.name == "Burning Glyph") EXECUTE_BURNING_GLYPH(caster, spell);
        else if (spell.name == "Repulsion Glyph") EXECUTE_REPULSION_GLYPH(caster, spell);
        else if (spell.name == "Dazzling") EXECUTE_DAZZLING(targetBlock, spell);
        else if (spell.name == "Bontao") EXECUTE_BONTAO(caster, spell);
        else if (spell.name == "Titanic Hit") EXECUTE_TITANIC_HIT(caster, targetBlock);
        else if (spell.name == "Telluric Wave") EXECUTE_TELLURIC_WAVE(caster, spell);
        else if (spell.name == "Polarity") EXECUTE_POLARITY(caster, spell);
        else if (spell.name == "Stratega") EXECUTE_STRATEGA(caster, targetBlock, spell);
        else if (spell.name == "Overcharge") EXECUTE_OVERCHARGE(caster, spell);
        else if (spell.name == "Striking Meteor") EXECUTE_STRIKING_METEOR(caster, targetBlock, spell);
        else if (spell.name == "Aerial Wave") EXECUTE_AERIAL_WAVE(caster, targetBlock);
        else if (spell.name == "Selective Word") EXECUTE_SELECTIVE_WORD(caster, targetBlock);
        else if (spell.name == "Striking Word") EXECUTE_STRIKING_WORD(caster, targetBlock, spell);
        else if (spell.name == "Recovery Word") EXECUTE_RECOVERY_WORD(targetBlock, spell);
        else if (spell.name == "Preventing Word") EXECUTE_PREVENTING_WORD(caster, targetBlock, spell);
        else if (spell.name == "Agonising Word") EXECUTE_AGONISING_WORD(caster, targetBlock);
        else if (spell.name == "Furious Word") EXECUTE_FURIOUS_WORD(caster, targetBlock, spell);

        // ADD HERE ELSE IF (...) ...
        else Debug.LogError("Effect for " + spell.name + " has not implemented yet");
    }

    public static void EXECUTE_JUMP(Character caster, Block targetBlock) {
        Debug.Log("Jump for " + caster.name);
        if (caster.connectedCell.GetComponent<Block>() != null) {
            caster.connectedCell.GetComponent<Block>().linkedObject = null;
        }
        caster.connectedCell = targetBlock.gameObject;
        Debug.Log("Connected cell: " + caster.connectedCell.GetComponent<Block>().coordinate.display());
        targetBlock.linkedObject = caster.gameObject;
        Debug.Log("Connected player: " + targetBlock.linkedObject.GetComponent<Character>().name);
        Vector2 newPosition = Coordinate.getPosition(targetBlock.coordinate);
        caster.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
        caster.setZIndex(targetBlock);
    }

    public static void EXECUTE_POUNDING(Block targetBlock, Spell s) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 30) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            if (target != null)
                target.addEvent(new PoundingEvent("Pounding", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_AGITATION(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target != null) {
            AgitationEvent agitationEvent = new AgitationEvent("Agitation", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
            target.addEvent(agitationEvent);
            if (target.name == s.link.name && target.team == s.link.team) {
                agitationEvent.useIstantanely();
            }
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
        if (target != null)
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
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 50) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            if (target != null) {
                ComposureEvent ce = new ComposureEvent("Composure", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
                ce.useIstantanely();
                target.addEvent(ce);
            }
        }
    }

    public static void EXECUTE_VIRUS(Character caster, Spell s, Block targetBlock) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character ch in ut_getAllies(caster)) {
            ch.receiveHeal(calculateDamage(caster, target, s) / 2);
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
        BowSkill bs = new BowSkill("Bow Skill", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(bs);
        bs.useIstantanely();
    }

    public static void EXECUTE_SLOW_DOWN_ARROW(Block targetBlock, Spell s) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
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
        ut_repels(caster, targetBlock, 3);
    }

    public static void EXECUTE_BARRICADE_SHOT(Character caster, Block targetBlock, Spell s) {
        Coordinate casterCoord = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetCoord = targetBlock.coordinate;
        if (casterCoord.column == targetCoord.column || casterCoord.row == targetCoord.row) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new BarricadeShotEvent("Barricade Shot", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_SENTINEL(Character caster, Spell s) {
        SentinelEvent sentinelEvent = new SentinelEvent("Sentinel", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(sentinelEvent);
        sentinelEvent.useIstantanely();
    }

    public static void EXECUTE_CRITICAL_SHOOTING(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        CriticalShootingEvent cs = new CriticalShootingEvent("Critical Shooting", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(cs);
        if (target.name == s.link.name && target.team == s.link.team)
            cs.useIstantanely();
    }

    public static void EXECUTE_EXODUS(Character caster, Block targetBlock, Spell s) {
        Coordinate casterCoord = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetCoord = targetBlock.coordinate;
        if (casterCoord.row == targetCoord.row && casterCoord.column < targetCoord.column) {
            // jump to the right
            Block toJump = Map.Instance.getBlock(new Coordinate(targetCoord.row, targetCoord.column + 1));
            if (toJump == null) return;
            if (toJump.linkedObject != null) return;
            if (caster.connectedCell.GetComponent<Block>() != null)
                caster.connectedCell.GetComponent<Block>().linkedObject = null;
            caster.connectedCell = toJump.gameObject;
            toJump.linkedObject = caster.gameObject;
            Vector2 newPosition = Coordinate.getPosition(toJump.coordinate);
            caster.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
        } else if (casterCoord.row == targetCoord.row && casterCoord.column > targetCoord.column) {
            // jump to the left
            Block toJump = Map.Instance.getBlock(new Coordinate(targetCoord.row, targetCoord.column - 1));
            if (toJump == null) return;
            if (toJump.linkedObject != null) return;
            if (caster.connectedCell.GetComponent<Block>() != null)
                caster.connectedCell.GetComponent<Block>().linkedObject = null;
            caster.connectedCell = toJump.gameObject;
            toJump.linkedObject = caster.gameObject;
            Vector2 newPosition = Coordinate.getPosition(toJump.coordinate);
            caster.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
        } else if (casterCoord.column == targetCoord.column && casterCoord.row > targetCoord.row) {
            // upper jump
            Block toJump = Map.Instance.getBlock(new Coordinate(targetCoord.row-1, targetCoord.column));
            if (toJump == null) return;
            if (toJump.linkedObject != null) return;
            if (caster.connectedCell.GetComponent<Block>() != null)
                caster.connectedCell.GetComponent<Block>().linkedObject = null;
            caster.connectedCell = toJump.gameObject;
            toJump.linkedObject = caster.gameObject;
            Vector2 newPosition = Coordinate.getPosition(toJump.coordinate);
            caster.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
        } else if (casterCoord.column == targetCoord.column && casterCoord.row < targetCoord.row) {
            // down jump
            Block toJump = Map.Instance.getBlock(new Coordinate(targetCoord.row + 1, targetCoord.column));
            if (toJump == null) return;
            if (toJump.linkedObject != null) return;
            if (caster.connectedCell.GetComponent<Block>() != null)
                caster.connectedCell.GetComponent<Block>().linkedObject = null;
            caster.connectedCell = toJump.gameObject;
            toJump.linkedObject = caster.gameObject;
            Vector2 newPosition = Coordinate.getPosition(toJump.coordinate);
            caster.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
        }
    }

    public static void EXECUTE_CONVULSION(Character caster, Block targetBlock) {
        ut_repels(caster, targetBlock, 2);
    }

    public static void EXECUTE_THERAPY(Character caster, Block targetBlock) {
        ut_attracts(caster, targetBlock, 1);
    }

    public static void EXECUTE_ODYSSEY(Character caster) {
        Block actual = caster.connectedCell.GetComponent<Block>();
        Coordinate start = actual.coordinate;
        List<Block> toValuate = new List<Block>();
        for (int i = -5; i <= 5; i++) {
            for (int j = -5; j <= 5; j++) {
                if (i == 0 && j == 0) continue; // start position
                Block temp = Map.Instance.getBlock(new Coordinate(start.row + i, start.column + j));
                if (temp == null) continue;
                if (temp.linkedObject != null) continue;
                toValuate.Add(temp);
            }
        }
        if (toValuate.Count > 0) {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int index_chosen = UnityEngine.Random.Range(0, toValuate.Count);
            Block chosen = toValuate[index_chosen];
            if (caster.connectedCell.GetComponent<Block>() != null)
                caster.connectedCell.GetComponent<Block>().linkedObject = null;
            caster.connectedCell = chosen.gameObject;
            chosen.linkedObject = caster.gameObject;
            Vector2 newPosition = Coordinate.getPosition(chosen.coordinate);
            caster.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
        }
    }

    public static void EXECUTE_TRANSPOSITION(Character caster, Block targetBlock) {
        Block casterBlock = caster.connectedCell.GetComponent<Block>();
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        casterBlock.linkedObject = null;
        targetBlock.linkedObject = null;
        caster.connectedCell = targetBlock.gameObject;
        caster.setZIndex(targetBlock);
        target.connectedCell = casterBlock.gameObject;
        target.setZIndex(casterBlock);
        casterBlock.linkedObject = target.gameObject;
        targetBlock.linkedObject = caster.gameObject;
        Vector2 targetNewPosition = Coordinate.getPosition(targetBlock.coordinate);
        Vector2 casterNewPosition = Coordinate.getPosition(casterBlock.coordinate);
        caster.transform.position = new Vector3(targetNewPosition.x, targetNewPosition.y, -20);
        target.transform.position = new Vector3(casterNewPosition.x, casterNewPosition.y, -20);
    }

    public static void EXECUTE_ATTRACTION(Character caster, Block targetBlock) {
        ut_attracts(caster, targetBlock, 6);
    }

    public static void EXECUTE_DESOLATION(Block targetBlock, Spell s) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 25) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new DesolationEvent("Desolation", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_BERSERK(Character caster, Spell s) {
        BerserkEvent se = new BerserkEvent("Berserk", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        se.useIstantanely();
        caster.addEvent(se);
    }

    public static void EXECUTE_MUTILATION(Character caster, Spell s) {
        MutilationEvent mut = new MutilationEvent("Mutilation", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(mut);
        if (caster.name == s.link.name && caster.team == s.link.team)
            mut.useIstantanely();
    }

    public static void EXECUTE_INFLUX(Character caster, Block targetBlock) {
        ut_attracts(caster, targetBlock, 2);
    }

    public static void EXECUTE_TRANSFUSION(Character caster, Spell s) {
        Coordinate a = caster.connectedCell.GetComponent<Block>().coordinate;
        foreach (Character ch in ut_getAllies(caster)) {
            Coordinate b = ch.connectedCell.GetComponent<Block>().coordinate;
            if (ut_isNearOf(a, b, 6)) {
                ch.receiveHeal(100);
            }
        }
    }

    public static void EXECUTE_SACRIFICE(Character caster, Block targetBlock, Spell s) {
        SacrificeEvent se = new SacrificeEvent("Sacrifice", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, targetBlock.linkedObject.GetComponent<Character>());
        caster.addEvent(se);
        se.useIstantanely();
    }

    public static void EXECUTE_SMELL(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        SmellEvent se = new SmellEvent("Smell", target, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        target.addEvent(se);
        if (target.name == s.link.name && target.team == s.link.team)
            se.useIstantanely();
    }

    public static void EXECUTE_HEADS_OR_TAILS(Block targetBlock, Spell s) {
        targetBlock.linkedObject.GetComponent<Character>().addEvent(new HeadOrTailEvent("Heads or Tails", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_ALL_OR_NOTHING(Block targetBlock, Spell s) {
        targetBlock.linkedObject.GetComponent<Character>().addEvent(new AllOrNothingEvent("All or Nothing", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_CLAW_OF_CEANGAL(Character caster, Block targetBlock) {
        ut_repels(caster, targetBlock, 2);
    }

    public static void EXECUTE_GODSEND(Block targetBlock, Spell s) {
        GodsendEvent ge = new GodsendEvent("Godsend", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        targetBlock.linkedObject.GetComponent<Character>().addEvent(ge);
        ge.useIstantanely();
    }

    public static void EXECUTE_FELINE_SENSE(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        Coordinate a = targetBlock.coordinate;
        List<Character> heroes = ut_getAllies(target);
        heroes.AddRange(ut_getEnemies(target));
        foreach (Character c in heroes) {
            Coordinate b = c.connectedCell.GetComponent<Block>().coordinate;
            if (ut_isNearOf(a, b, 3)) {
                UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
                int v = UnityEngine.Random.Range(1, 3);
                Debug.Log("Value " + v);
                if (v == 1)
                    target.receiveHeal(50);
                else Debug.Log("Feline sense failed");
            }
        }
    }

    public static void EXECUTE_ROULETTE(Character caster, Spell s) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int chosenRandomId = UnityEngine.Random.Range(1, 16); // 1 to 15
        List<RouletteEvent> rouletteEvents = new List<RouletteEvent>();
        foreach (Character ch in ut_getAllies(caster)) {
            RouletteEvent re = new RouletteEvent("Roulette", ch, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon, chosenRandomId);
            ch.addEvent(re);
            if (chosenRandomId == 5 || chosenRandomId == 6 || chosenRandomId == 7 || chosenRandomId == 8 || chosenRandomId == 9 || chosenRandomId == 10 ||
                chosenRandomId == 11 || chosenRandomId == 12 || chosenRandomId == 13 || chosenRandomId == 14)
                re.useIstantanely();
        }
    }

    public static void EXECUTE_TIME_RIFT(Character caster, Block targetBlock, Spell s) {
        List<Character> lc = TurnsManager.Instance.turns;
        List<Character> allies = ut_getAllies(caster);
        if (allies.Count > 0) {
            Character toTeleport = allies[UnityEngine.Random.Range(0, allies.Count)];
            EXECUTE_JUMP(toTeleport, targetBlock);
        }
    }

    public static void EXECUTE_SANDGLASS(Block targetBlock, Spell s) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 90) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new SandglassEvent("Sandglass", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_REWIND(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new RewindEvent("Rewind", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_CLOCK(Block targetBlock, Spell s) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 20) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new ClockEvent("Clock", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_TIME_THEFT(Character caster, Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new TimeTheftEvent("Time Theft", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster));
    }

    public static void EXECUTE_HAZINESS(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new HazinessEvent("Haziness", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_SLOW_DOWN(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new SlowDownEvent("Slow Down", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_GEAR(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new GearEvent("Gear", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_RESTART(Spell s) {
        foreach(Tuple<Character, Block> t in TurnsManager.spawnPositions) {
            if (t.Item1.isDead) continue;
            EXECUTE_JUMP(t.Item1, t.Item2);
        }
    }

    public static void EXECUTE_STAMPEDE(Character caster, Block targetBlock, Spell s) {
        EXECUTE_JUMP(caster, targetBlock);
        StampedeEvent se = new StampedeEvent("Stampede", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
        caster.addEvent(se);
        se.useIstantanely();
    }

    public static void EXECUTE_CAPERING(Character caster, Block targetBlock, Spell s) {
        EXECUTE_JUMP(caster, targetBlock);
        Coordinate c = targetBlock.coordinate;
        List<Character> adj_heroes = ut_getAdjacentHeroes(c);
        foreach(Character adj in adj_heroes)
            adj.inflictDamage(calculateDamage(caster, adj, s));
    }

    public static void SWITCH_COWARD_MASK(Character caster, Spell s) {
        if (caster.getEventSystem().getEvents("Coward Mask").Count == 0) {
            // Activate
            CowardMask cm = new CowardMask("Coward Mask", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
            caster.addEvent(cm);
            cm.useIstantanely();
        } else {
            // Deactivate
            caster.getEventSystem().removeEvents("Coward Mask");
        }
    }

    public static void SWITCH_PSYCHOPATH_MASK(Character caster, Spell s) {
        if (caster.getEventSystem().getEvents("Psychopath Mask").Count == 0) {
            // Activate
            PsychopathMask cm = new PsychopathMask("Psychopath Mask", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
            caster.addEvent(cm);
            cm.useIstantanely();
        } else {
            // Deactivate
            caster.getEventSystem().removeEvents("Psychopath Mask");
        }
    }

    public static void SWITCH_TIRELESS_MASK(Character caster, Spell s) {
        if (caster.getEventSystem().getEvents("Tireless Mask").Count == 0) {
            // Activate
            TirelessMask cm = new TirelessMask("Tireless Mask", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
            caster.addEvent(cm);
            cm.useIstantanely();
        } else {
            // Deactivate
            caster.getEventSystem().removeEvents("Tireless Mask");
        }
    }

    public static void EXECUTE_APATHY(Block targetBlock, Spell s) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 35) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new ApathyEvent("Apathy", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_TORTUGA(Block targetBlock, Spell s) {
        Character c = targetBlock.linkedObject.GetComponent<Character>();
        TortugaEvent se = new TortugaEvent("Tortuga", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        c.addEvent(se);
        se.useIstantanely();
    }

    public static void EXECUTE_FURIA(Character caster, Block targetBlock) {
        ut_comesCloser(caster, targetBlock, 2);
    }

    public static void EXECUTE_COMEDY(Character caster, Block targetBlock) {
        ut_comesCloser(caster, targetBlock, 2);
        ut_repels(caster, targetBlock, 4);
    }

    public static void EXECUTE_APOSTASY(Character caster, Block targetBlock) {
        ut_repels(caster, targetBlock, 2);
    }

    public static void EXECUTE_PUDDLE_GLYPH(Character caster, Spell s) {
        foreach(Character c in ut_getEnemies(caster)) {
            if (ut_isNearOf(caster, c, 4))
                c.addEvent(new PuddleGlyphEvent("Puddle Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
        }
    }

    public static void EXECUTE_AGGRESSIVE_GLYPH(Character caster, Spell s) {
        foreach (Character c in ut_getEnemies(caster)) {
            if (ut_isNearOf(caster, c, 3))
                c.addEvent(new AggressiveGlyphEvent("Aggressive Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
        }
    }

    public static void EXECUTE_LIGHTNESS(Character caster, Spell s) {
        foreach (Character c in ut_getAlliesWithCaster(caster)) {
            if (ut_isNearOf(caster, c, 3)) {
                if (caster.Equals(c)) {
                    LightnessEvent le = new LightnessEvent("Lightness", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
                    caster.addEvent(le);
                    le.useIstantanely();
                } else c.addEvent(new LightnessEvent("Lightness", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
            }
        }
    }

    public static void EXECUTE_PROTECTIVE_GLYPH(Character caster, Spell s) {
        foreach(Character c in ut_getAllies(caster))
            if (ut_isNearOf(c, caster, 3)) {
                ProtectiveGlyphEvent pg = new ProtectiveGlyphEvent("Protective Glyph", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, 0, caster, s);
                c.addEvent(pg);
                pg.useIstantanely();
            }
        foreach (Character c in ut_getEnemies(caster))
            if (ut_isNearOf(c, caster, 3))
                c.addEvent(new ProtectiveGlyphEvent("Protective Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, 1, caster, s));
    }

    public static void EXECUTE_PERCEPTION_GLYPH(Character caster, Spell s) {
        foreach (Character c in ut_getEnemies(caster))
            if (ut_isNearOf(c, caster, 5))
                c.addEvent(new PerceptionGlyphEvent("Perception Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
    }

    public static void EXECUTE_BARRICADE(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        BarricadeEvent be = new BarricadeEvent("Barricade", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(be);
        be.useIstantanely();
    }

    public static void EXECUTE_REPULSION_GLYPH(Character caster, Spell s) {
        caster.addEvent(new RepulsionGlyphEvent("Repulsion Glyph", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_BURNING_GLYPH(Character caster, Spell s) {
        foreach (Character c in ut_getEnemies(caster)) {
            if (ut_isNearOf(caster, c, 4))
                c.addEvent(new BurningGlyphEvent("Burning Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
        }
    }

    public static void EXECUTE_DAZZLING(Block targetBlock, Spell s) {
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 15) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            if (target != null)
                target.addEvent(new DazzlingEvent("Dazzling", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
        }
    }

    public static void EXECUTE_FORTIFICATION(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new FortificationEvent("Fortification", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_BONTAO(Character caster, Spell s) {
        foreach (Character c in ut_getAllies(caster)) {
            c.receiveHeal(s.damage);
        }
    }

    public static void EXECUTE_TITANIC_HIT(Character caster, Block targetBlock) {
        ut_comesCloser(caster, targetBlock, 4);
    }

    public static void EXECUTE_TELLURIC_WAVE(Character caster, Spell s) {
        int remaining_pm = caster.getActualPM();
        if (remaining_pm > 0)
            caster.decrementPM(remaining_pm);
        int bonus_damage = remaining_pm * 15;
        foreach (Character c in ut_getEnemies(caster)) {
            if (ut_isNearOf(caster, c, 5)) {
                int damage = Spell.calculateDamage(caster, c, s);
                damage += bonus_damage;
                c.inflictDamage(damage);
            }
        }
    }

    public static void EXECUTE_POLARITY(Character caster, Spell s) {
        PolarityEvent pe = new PolarityEvent("Polarity", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(pe);
        pe.useIstantanely();
    }

    public static void EXECUTE_STRATEGA(Character caster, Block targetBlock, Spell s) {
        ut_repelsCaster(caster, targetBlock, 1);
        targetBlock.linkedObject.GetComponent<Character>().addEvent(
            new StrategaEvent("Stratega", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon)
        );
    }

    public static void EXECUTE_OVERCHARGE(Character caster, Spell s) {
        int remaining_pa = caster.getActualPA();
        if (remaining_pa > 0)
            caster.decrementPA(remaining_pa);
        int bonus_shield = remaining_pa * 40;
        caster.receiveShield(s.damage + bonus_shield);
    }

    public static void EXECUTE_STRIKING_METEOR(Character caster, Block targetBlock, Spell s) {
        EXECUTE_JUMP(caster, targetBlock);
        foreach(Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(caster, enemy, 3))
                enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
    }

    public static void EXECUTE_AERIAL_WAVE(Character caster, Block targetBlock) {
        ut_repelsCaster(caster, targetBlock, 3);
        ut_repels(caster, targetBlock, 3);
    }

    public static void EXECUTE_SELECTIVE_WORD(Character caster, Block targetBlock) {
        List<Character> heroes = ut_getAdjacentHeroes(targetBlock.coordinate);
        foreach (Character c in heroes)
            if (!c.isEnemyOf(caster))
                c.receiveHeal(30);
    }

    public static void EXECUTE_STRIKING_WORD(Character caster, Block targetBlock, Spell s) {
        foreach (Character ally in ut_getAllies(caster))
            if (ut_isNearOf(ally, targetBlock.linkedObject.GetComponent<Character>(), 3))
                ally.addEvent(new StrikingWordEvent("Striking Word", ally, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_RECOVERY_WORD(Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new RecoveryWordEvent("Recovery Word", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_PREVENTING_WORD(Character caster, Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        PreventingWordEvent pw = new PreventingWordEvent("Preventing Word", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(pw);
        if (target.Equals(caster)) pw.useIstantanely();
    }

    public static void EXECUTE_AGONISING_WORD(Character caster, Block targetBlock) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        List<Character> adjs = ut_getAdjacentHeroes(targetBlock.coordinate);
        foreach(Character c in adjs) {
            c.inflictDamage(10);
            ut_repels(target, c.connectedCell.GetComponent<Block>(), 1);
        }
    }

    public static void EXECUTE_FURIOUS_WORD(Character caster, Block targetBlock, Spell s) {
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new FuriousWordEvent("Furious Word", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
        List<Character> adjs = ut_getAdjacentHeroes(targetBlock.coordinate);
        foreach (Character c in adjs)
            c.inflictDamage(100);
    }

    #endregion

    #region EVENT BONUSES

    public static int BONUS_ACCUMULATION = 4;
    public static int BONUS_WRATH = 120;
    public static int BONUS_BOW_SKILL = 12;
    public static int BONUS_ATONEMENT_ARROW = 36;
    public static int BONUS_DECIMATION = 48;
    public static int BONUS_SHADOWYBEAM = 13;

    public static int EVENT_BONUS_BASE_DAMAGE(Character caster, Character targetch, Spell s) {
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
        } else if (caster.name == "Pilobouli" && s.name == "Decimation") {
            if (caster.hasActivedSacrifice && caster.getTotalHP() * 50 / 100 > caster.getActualHP()) {
                Debug.Log("Bonus decimation!");
                return BONUS_DECIMATION;
            } else return 0;
        } else if (caster.name == "Rabiote" && s.name == "Bluff") {
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int diceResult = UnityEngine.Random.Range(1, 7);
            if (diceResult == 1) return 5;
            if (diceResult == 2) return 15;
            if (diceResult == 3) return 25;
            if (diceResult == 4) return 30;
            if (diceResult == 5) return 35;
            if (diceResult == 6) return 45;
            else return 0;
        } else if (caster.name == "Chrona" && s.name == "Shadowy Beam") {
            Coordinate target = targetch.connectedCell.GetComponent<Block>().coordinate;
            if (Map.Instance.getBlock(new Coordinate(target.row, target.column + 1)) != null || Map.Instance.getBlock(new Coordinate(target.row, target.column - 1)) != null ||
                Map.Instance.getBlock(new Coordinate(target.row + 1, target.column)) != null || Map.Instance.getBlock(new Coordinate(target.row - 1, target.column)) != null)
                return BONUS_SHADOWYBEAM;
            else return 0;
        } else return 0;
    }

    #endregion

    #region UTILITIES

    public static List<Character> ut_getAllies(Character caster) {
        List<Character> toReturn = new List<Character>();
        foreach (Character ch in TurnsManager.Instance.turns) {
            if (ch.isDead) continue;
            if (ch.team == caster.team && ch.name != caster.name) {
                toReturn.Add(ch);
            }
        }
        return toReturn;
    }

    public static List<Character> ut_getAlliesWithCaster(Character caster) {
        List<Character> toReturn = new List<Character>();
        foreach (Character ch in TurnsManager.Instance.turns) {
            if (ch.isDead) continue;
            if (ch.team == caster.team) {
                toReturn.Add(ch);
            }
        }
        return toReturn;
    }

    public static List<Character> ut_getEnemies(Character caster) {
        List<Character> toReturn = new List<Character>();
        foreach (Character ch in TurnsManager.Instance.turns) {
            if (ch.isDead) continue;
            if (ch.isEnemyOf(caster)) {
                toReturn.Add(ch);
            }
        }
        return toReturn;
    }

    public static void ut_repels(Character caster, Block targetBlock, int numberOfCellsToMove) {
        List<Block> path = new List<Block>();
        Coordinate casterPosition = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetPosition = targetBlock.coordinate;
        if (targetPosition.row > casterPosition.row) {
            // target is down
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row + i, targetPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.row < casterPosition.row) {
            // target is up
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row - i, targetPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column > casterPosition.column) {
            // target is on the right
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row, targetPosition.column + i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column < casterPosition.column) {
            // target is on the left
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row, targetPosition.column - i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        }
        if (path.Count > 0)
            targetBlock.linkedObject.GetComponent<Character>().setPath(path); // move the enemy
    }

    public static void ut_repelsCaster(Character caster, Block targetBlock, int numberOfCellsToMove) {
        List<Block> path = new List<Block>();
        Coordinate casterPosition = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetPosition = targetBlock.coordinate;
        if (targetPosition.row > casterPosition.row) {
            // target is down
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(casterPosition.row - i, casterPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.row < casterPosition.row) {
            // target is up
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(casterPosition.row + i, casterPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column > casterPosition.column) {
            // target is on the right
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(casterPosition.row, casterPosition.column - i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column < casterPosition.column) {
            // target is on the left
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(casterPosition.row, casterPosition.column + i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        }
        if (path.Count > 0)
            caster.setPath(path); // move the caster
    }

    public static void ut_attracts(Character caster, Block targetBlock, int numberOfCellsToMove) {
        List<Block> path = new List<Block>();
        Coordinate casterPosition = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetPosition = targetBlock.coordinate;
        Debug.Log("Coordinate hit: " + targetPosition.display());
        if (targetPosition.row > casterPosition.row) {
            Debug.Log("Target is down");
            // target is down
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row - i, targetPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.row < casterPosition.row) {
            Debug.Log("Target is up");
            // target is up
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row + i, targetPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column > casterPosition.column) {
            Debug.Log("Target is on the right");
            // target is on the right
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row, targetPosition.column - i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column < casterPosition.column) {
            Debug.Log("Target is on the left");
            // target is on the left
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(targetPosition.row, targetPosition.column + i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        }
        if (path.Count > 0)
            targetBlock.linkedObject.GetComponent<Character>().setPath(path); // move the enemy
    }

    public static void ut_comesCloser(Character caster, Block targetBlock, int numberOfCellsToMove) {
        List<Block> path = new List<Block>();
        Coordinate casterPosition = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetPosition = targetBlock.coordinate;
        Debug.Log("Coordinate hit: " + targetPosition.display());
        if (targetPosition.row > casterPosition.row) {
            Debug.Log("Target is down");
            // target is down
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(casterPosition.row + i, casterPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.row < casterPosition.row) {
            Debug.Log("Target is up");
            // target is up
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(casterPosition.row - i, casterPosition.column));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column > casterPosition.column) {
            Debug.Log("Target is on the right");
            // target is on the right
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(casterPosition.row, casterPosition.column + i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        } else if (targetPosition.column < casterPosition.column) {
            Debug.Log("Target is on the left");
            // target is on the left
            for (int i = 1; i <= numberOfCellsToMove; i++) {
                Block pointed = Map.Instance.getBlock(new Coordinate(casterPosition.row, casterPosition.column - i));
                if (pointed == null) break;
                if (pointed.linkedObject == null) path.Add(pointed);
                else break;
            }
        }
        if (path.Count > 0)
            caster.setPath(path); // move the character
    }

    public static bool ut_isNearOf(Coordinate a, Coordinate b, int cells) {
        int dist_row = Mathf.Abs(a.row - b.row);
        int dist_col = Mathf.Abs(a.column - b.column);
        return (dist_row + dist_col <= cells);
    }

    public static bool ut_isNearOf(Character ac, Character bc, int cells) {
        Coordinate a = ac.connectedCell.GetComponent<Block>().coordinate;
        Coordinate b = bc.connectedCell.GetComponent<Block>().coordinate;
        int dist_row = Mathf.Abs(a.row - b.row);
        int dist_col = Mathf.Abs(a.column - b.column);
        return (dist_row + dist_col <= cells);
    }

    public static List<Character> ut_getAdjacentHeroes(Coordinate c) {
        Block adjacent = Map.Instance.getBlock(new Coordinate(c.row, c.column + 1));
        List<Character> toReturn = new List<Character>();
        if (adjacent != null)
            if (adjacent.linkedObject != null)
                toReturn.Add(adjacent.linkedObject.GetComponent<Character>());
        adjacent = Map.Instance.getBlock(new Coordinate(c.row, c.column - 1));
        if (adjacent != null)
            if (adjacent.linkedObject != null)
                toReturn.Add(adjacent.linkedObject.GetComponent<Character>());
        adjacent = Map.Instance.getBlock(new Coordinate(c.row + 1, c.column));
        if (adjacent != null)
            if (adjacent.linkedObject != null)
                toReturn.Add(adjacent.linkedObject.GetComponent<Character>());
        adjacent = Map.Instance.getBlock(new Coordinate(c.row - 1, c.column));
        if (adjacent != null)
            if (adjacent.linkedObject != null)
                toReturn.Add(adjacent.linkedObject.GetComponent<Character>());
        return toReturn;
    }

    #endregion
}