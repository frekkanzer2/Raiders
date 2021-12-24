﻿using System;
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
        Line,
        Diagonal
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
    public bool isJump; // done
    public bool isSummon;

    public void OnPreviewPressed() {
        Debug.Log("Pressed spell " + name);
        link.displayAttackCells(link, this);
    }

    public static void payCost(Character caster, Spell spell) {
        if (caster is Evocation)
            if (((Evocation)caster).isTurrect && caster.getEventSystem().getEvents("Harpooner Charge").Count > 0) {
                caster.getEventSystem().removeEvents("Harpooner Charge");
                return;
            }
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
        if (spell.isSummon && caster.summons.Count == caster.numberOfSummons) return false;
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
        if (!put_CheckArguments(new System.Object[] { caster, target, spell })) return 0;
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
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, spell })) return;
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
            if (!(caster is Monster))
                SPELL_SPECIALIZATION(caster, targetBlock, spell);
            else
                MONSTER_SPELL_SPECIALIZATION(caster, targetBlock, spell);
        }
    }

    public bool isOffensiveSpell() {
        return (this.element == Element.Air || this.element == Element.Fire || this.element == Element.Earth || this.element == Element.Water);
    }

    #region SPELL SPECIALIZATIONS

    public static void SPELL_SPECIALIZATION(Character caster, Block targetBlock, Spell spell) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, spell })) return;
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
        else if (spell.name == "Retreat Arrow" || spell.name == "Tricky Blow") EXECUTE_RETREAT_ARROW(caster, targetBlock);
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
        else if (spell.name == "Claw of Ceangal" || spell.name == "Haunting Magic") EXECUTE_CLAW_OF_CEANGAL(caster, targetBlock);
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
        else if (spell.name == "Stimulating Word") EXECUTE_STIMULATING_WORD(targetBlock, spell);
        else if (spell.name == "Paralysing Word") EXECUTE_PARALYSING_WORD(targetBlock, spell);
        else if (spell.name == "Galvanising Word") EXECUTE_GALVANISING_WORD(caster, spell);
        else if (spell.name == "Call of Bwork Mage") SUMMONS_BWORK_MAGE(caster, targetBlock);
        else if (spell.name == "Call of Craqueleur") SUMMONS_CRAQUELEUR(caster, targetBlock);
        else if (spell.name == "Call of Dragonnet") SUMMONS_DRAGONNET(caster, targetBlock);
        else if (spell.name == "Call of Tofu") SUMMONS_TOFU(caster, targetBlock);
        else if (spell.name == "Call of Gobball") SUMMONS_GOBBALL(caster, targetBlock);
        else if (spell.name == "Call of Prespic") SUMMONS_PRESPIC(caster, targetBlock);
        else if (spell.name == "Call of Pandawasta") SUMMONS_PANDAWASTA(caster, targetBlock);
        else if (spell.name == "Call of Bamboo") SUMMONS_BAMBOO(caster, targetBlock);
        else if (spell.name == "Call of The Block") SUMMONS_THE_BLOCK(caster, targetBlock);
        else if (spell.name == "Call of Sacrificial Doll") SUMMONS_SACRIFICIAL(caster, targetBlock);
        else if (spell.name == "Call of Madoll") SUMMONS_MADOLL(caster, targetBlock);
        else if (spell.name == "Call of Tree") SUMMONS_TREE(caster, targetBlock);
        else if (spell.name == "Call of Inflatable") SUMMONS_INFLATABLE(caster, targetBlock);
        else if (spell.name == "Sedimentation") EXECUTE_SEDIMENTATION(targetBlock, spell);
        else if (spell.name == "Sting") EXECUTE_STING(targetBlock, spell);
        else if (spell.name == "Pleasure") EXECUTE_PLEASURE(targetBlock, spell);
        else if (spell.name == "Whip") EXECUTE_WHIP(targetBlock, spell);
        else if (spell.name == "Communion") EXECUTE_COMMUNION(targetBlock, spell);
        else if (spell.name == "Summoner Fury") EXECUTE_SUMMONER_FURY(targetBlock, spell);
        else if (spell.name == "Chamrak") EXECUTE_CHAMRAK(caster, targetBlock, spell);
        else if (spell.name == "Waterfall") EXECUTE_WATERFALL(caster, targetBlock);
        else if (spell.name == "Blow-Out") EXECUTE_BLOW_OUT(caster, spell);
        else if (spell.name == "Eviction") EXECUTE_EVICTION(caster, targetBlock);
        else if (spell.name == "Pandjiu") EXECUTE_PANDJIU(caster, targetBlock);
        else if (spell.name == "Explosive Flask") EXECUTE_EXPLOSIVE_FLASK(caster, targetBlock, spell);
        else if (spell.name == "Contagion") EXECUTE_CONTAGION(caster, targetBlock, spell);
        else if (spell.name == "Nature Poison") EXECUTE_NATURE_POISON(caster, targetBlock, spell);
        else if (spell.name == "Earthquake") EXECUTE_EARTHQUAKE(caster, spell);
        else if (spell.name == "Doll Sacrifice") EXECUTE_DOLL_SACRIFICE(caster);
        else if (spell.name == "Doll Scream") EXECUTE_DOLL_SCREAM(caster, targetBlock, spell);
        else if (spell.name == "Explobombe") SUMMONS_EXPLOBOMBE(caster, targetBlock, spell);
        else if (spell.name == "Tornabombe") SUMMONS_TORNABOMBE(caster, targetBlock, spell);
        else if (spell.name == "Waterbombe") SUMMONS_WATERBOMBE(caster, targetBlock, spell);
        else if (spell.name == "Living Shovel") SUMMONS_LIVING_SHOVEL(caster, targetBlock);
        else if (spell.name == "Detonator") EXECUTE_DETONATOR(caster, targetBlock);
        else if (spell.name == "Powder") EXECUTE_POWDER(caster, targetBlock);
        else if (spell.name == "Kickback") EXECUTE_KICKBACK(caster, targetBlock);
        else if (spell.name == "Bomb Trick") EXECUTE_BOMB_TRICK(caster, targetBlock);
        else if (spell.name == "Deception") EXECUTE_DECEPTION(targetBlock, spell);
        else if (spell.name == "Trident") EXECUTE_TRIDENT(caster, targetBlock, spell);
        else if (spell.name == "Torrent") EXECUTE_TORRENT(caster, targetBlock, spell);
        else if (spell.name == "Assistance") EXECUTE_ASSISTANCE(caster, targetBlock);
        else if (spell.name == "Compass") EXECUTE_COMPASS(caster, targetBlock);
        else if (spell.name == "Harpooner Charge") EXECUTE_HARPOONER_CHARGE(caster, spell);
        else if (spell.name == "Harpooner") SUMMONS_SENTINEL_TURRECT(caster, targetBlock);
        else if (spell.name == "Tacturrect") SUMMONS_TACTICAL_TURRECT(caster, targetBlock);
        else if (spell.name == "Lifesaver") SUMMONS_GUARDIANA_TURRECT(caster, targetBlock);
        else if (spell.name == "Repulsion") EXECUTE_REPULSION(caster, targetBlock);
        else if (spell.name == "Vivacity") EXECUTE_VIVACITY(caster, targetBlock);
        else if (spell.name == "Mist") EXECUTE_MIST(caster, spell);
        else if (spell.name == "Double") SUMMONS_DOUBLE(caster, targetBlock);
        else if (spell.name == "Chaferfu") SUMMONS_CHAFERFU(caster, targetBlock);
        else if (spell.name == "Cruelty") EXECUTE_CRUELTY(caster, targetBlock, spell);
        else if (spell.name == "Perquisition") EXECUTE_PERQUISITION(caster, targetBlock, spell);
        else if (spell.name == "Larceny") EXECUTE_LARCENY(caster, targetBlock, spell);
        else if (spell.name == "Toxic Injection") EXECUTE_TOXIC_INJECTION(caster, targetBlock, spell);
        else if (spell.name == "Cut Throat") EXECUTE_CUT_THROAT(caster, spell);
        else if (spell.name == "Evasion") EXECUTE_EVASION(caster, spell);
        else if (spell.name == "Pull Out") EXECUTE_PULL_OUT(caster, spell);
        else if (spell.name == "Toolbox") EXECUTE_TOOLBOX(targetBlock, spell);
        else if (spell.name == "Tunneling") EXECUTE_TUNNELING(caster, targetBlock, spell);
        else if (spell.name == "Obsolescence") EXECUTE_OBSOLESCENCE(targetBlock, spell);
        else if (spell.name == "Fortune") EXECUTE_FORTUNE(targetBlock, spell);
        else if (spell.name == "Money Collection") EXECUTE_MONEY_COLLECTION(caster);
        else if (spell.name == "Power Unlocker") EXECUTE_POWER_UNLOCKER(caster, spell);

        // ADD HERE ELSE IF (...) ...
        else Debug.LogError("Effect for " + spell.name + " has not implemented yet");
    }

    public static void MONSTER_SPELL_SPECIALIZATION(Character caster, Block targetBlock, Spell spell) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, spell })) return;
        try {
            if (spell.name == "Brikocoop" || spell.name == "Brutocoop") EXECUTE_BRIKOCOOP(caster, spell);
            else if (spell.name == "Briko Assault" || spell.name == "Bruto Assault") EXECUTE_BRIKOASSAULT(caster, targetBlock, spell);
            else if (spell.name == "Briko Stimulation") EXECUTE_BRIKO_STIMULATION(caster, spell);
            else if (spell.name == "Sting") EXECUTE_STING(targetBlock, spell);
            else if (spell.name == "Wild Lash" || spell.name == "Breeze") EXECUTE_RETREAT_ARROW(caster, targetBlock);
            else if (spell.name == "Manifold Bramble") EXECUTE_MANIFOLD_BRAMBLE(caster, targetBlock, spell);
            else if (spell.name == "Bruto Stimulation") EXECUTE_BRUTO_STIMULATION(caster, spell);
            // ADD HERE ELSE IF (...) ...
            else Debug.LogError("Effect for " + spell.name + " has not implemented yet");
        } catch (Exception e) {
            Debug.LogError("Exception throws: " + e.StackTrace);
        }
    }

    #endregion

    #region CHARACTER SPELLS SPECIALIZATION

    public static void EXECUTE_JUMP(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!caster.canMovedByEffects) return;
        Debug.Log("Jump for " + caster.name);
        if (caster.connectedCell.GetComponent<Block>() != null) {
            caster.connectedCell.GetComponent<Block>().linkedObject = null;
        }
        caster.connectedCell = targetBlock.gameObject;
        targetBlock.linkedObject = caster.gameObject;
        Vector2 newPosition = Coordinate.getPosition(targetBlock.coordinate);
        caster.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
        caster.setZIndex(targetBlock);
    }

    public static void EXECUTE_POUNDING(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
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
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target != null) {
            AgitationEvent agitationEvent = new AgitationEvent("Agitation", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
            target.addEvent(agitationEvent);
            if (target.Equals(s.link)) {
                agitationEvent.useIstantanely();
            }
        }
    }

    public static void EXECUTE_ACCUMULATION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new AccumulationEvent("Accumulation", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_POWER(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        PowerEvent powerEvent = new PowerEvent("Power", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(powerEvent);
        if (target.Equals(s.link))
            powerEvent.useIstantanely();
    }

    public static void EXECUTE_DUEL(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        DuelEvent casterEvent = new DuelEvent("Duel", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
        casterEvent.useIstantanely();
        caster.addEvent(casterEvent);
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target != null)
            target.addEvent(new DuelEvent("Duel", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_IOP_WRATH(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new IopWrathEvent("Iop's Wrath", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_STRETCHING(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        StretchingEvent se = new StretchingEvent("Stretching", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        caster.addEvent(se);
        se.useIstantanely();
    }

    public static void EXECUTE_COMPOSURE(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character ch in ut_getAllies(caster)) {
            ch.receiveHeal(calculateDamage(caster, target, s) / 2);
        }
    }

    public static void EXECUTE_POWERFUL_SHOOTING(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        PowerfulShooting powerEvent = new PowerfulShooting("Powerful Shooting", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(powerEvent);
        if (target.Equals(s.link))
            powerEvent.useIstantanely();
    }

    public static void EXECUTE_BOW_SKILL(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        BowSkill bs = new BowSkill("Bow Skill", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(bs);
        bs.useIstantanely();
    }

    public static void EXECUTE_SLOW_DOWN_ARROW(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 60) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new SlowDownArrowEvent("Slow Down Arrow", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_ATONEMENT_ARROW(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new AtonementArrowEvent("Atonement Arrow", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_RETREAT_ARROW(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 3);
    }

    public static void EXECUTE_BARRICADE_SHOT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Coordinate casterCoord = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetCoord = targetBlock.coordinate;
        if (casterCoord.column == targetCoord.column || casterCoord.row == targetCoord.row) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new BarricadeShotEvent("Barricade Shot", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_SENTINEL(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        SentinelEvent sentinelEvent = new SentinelEvent("Sentinel", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(sentinelEvent);
        sentinelEvent.useIstantanely();
    }

    public static void EXECUTE_CRITICAL_SHOOTING(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        CriticalShootingEvent cs = new CriticalShootingEvent("Critical Shooting", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(cs);
        if (target.Equals(s.link))
            cs.useIstantanely();
    }

    public static void EXECUTE_EXODUS(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!caster.canMovedByEffects) return;
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
        caster.setZIndex(caster.connectedCell.GetComponent<Block>());
    }

    public static void EXECUTE_CONVULSION(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 2);
    }

    public static void EXECUTE_THERAPY(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 1);
    }

    public static void EXECUTE_ODYSSEY(Character caster) {
        if (!put_CheckArguments(new System.Object[] { caster })) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (!caster.canMovedByEffects || !targetBlock.linkedObject.GetComponent<Character>().canMovedByEffects) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 6);
    }

    public static void EXECUTE_DESOLATION(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 25) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new DesolationEvent("Desolation", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_BERSERK(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        BerserkEvent se = new BerserkEvent("Berserk", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        se.useIstantanely();
        caster.addEvent(se);
    }

    public static void EXECUTE_MUTILATION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        MutilationEvent mut = new MutilationEvent("Mutilation", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(mut);
        if (caster.Equals(s.link))
            mut.useIstantanely();
    }

    public static void EXECUTE_INFLUX(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 2);
    }

    public static void EXECUTE_TRANSFUSION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        Coordinate a = caster.connectedCell.GetComponent<Block>().coordinate;
        foreach (Character ch in ut_getAllies(caster)) {
            Coordinate b = ch.connectedCell.GetComponent<Block>().coordinate;
            if (ut_isNearOf(a, b, 6)) {
                ch.receiveHeal(100);
            }
        }
    }

    public static void EXECUTE_SACRIFICE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        SacrificeEvent se = new SacrificeEvent("Sacrifice", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, targetBlock.linkedObject.GetComponent<Character>());
        caster.addEvent(se);
        se.useIstantanely();
    }

    public static void EXECUTE_SMELL(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        SmellEvent se = new SmellEvent("Smell", target, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        target.addEvent(se);
        if (target.Equals(s.link))
            se.useIstantanely();
    }

    public static void EXECUTE_HEADS_OR_TAILS(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        targetBlock.linkedObject.GetComponent<Character>().addEvent(new HeadOrTailEvent("Heads or Tails", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_ALL_OR_NOTHING(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        targetBlock.linkedObject.GetComponent<Character>().addEvent(new AllOrNothingEvent("All or Nothing", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_CLAW_OF_CEANGAL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 2);
    }

    public static void EXECUTE_GODSEND(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        GodsendEvent ge = new GodsendEvent("Godsend", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        targetBlock.linkedObject.GetComponent<Character>().addEvent(ge);
        ge.useIstantanely();
    }

    public static void EXECUTE_FELINE_SENSE(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        Debug.Log("ROULETTE CASTER: " + caster.name);
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int chosenRandomId = UnityEngine.Random.Range(1, 16); // 1 to 15
        List<RouletteEvent> rouletteEvents = new List<RouletteEvent>();
        foreach (Character ch in ut_getAllies(caster)) {
            RouletteEvent re = new RouletteEvent("Roulette", ch, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon, chosenRandomId);
            ch.addEvent(re);
            Debug.Log("Adding event to: " + ch.name);
            if (chosenRandomId == 5 || chosenRandomId == 6 || chosenRandomId == 7 || chosenRandomId == 8 || chosenRandomId == 9 || chosenRandomId == 10 ||
                chosenRandomId == 11 || chosenRandomId == 12 || chosenRandomId == 13 || chosenRandomId == 14)
                re.useIstantanely();
        }
    }

    public static void EXECUTE_TIME_RIFT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        List<Character> lc = TurnsManager.Instance.turns;
        List<Character> allies = ut_getAllies(caster);
        if (allies.Count > 0) {
            Character toTeleport = allies[UnityEngine.Random.Range(0, allies.Count)];
            EXECUTE_JUMP(toTeleport, targetBlock);
        }
    }

    public static void EXECUTE_SANDGLASS(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 90) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new SandglassEvent("Sandglass", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_REWIND(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new RewindEvent("Rewind", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_CLOCK(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 20) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new ClockEvent("Clock", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_TIME_THEFT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new TimeTheftEvent("Time Theft", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster));
    }

    public static void EXECUTE_HAZINESS(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new HazinessEvent("Haziness", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_SLOW_DOWN(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new SlowDownEvent("Slow Down", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_GEAR(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new GearEvent("Gear", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_RESTART(Spell s) {
        if (!put_CheckArguments(new System.Object[] { s })) return;
        foreach (Tuple<Character, Block> t in TurnsManager.spawnPositions) {
            if (t.Item1.isDead || t.Item1.isEvocation) continue;
            if (t.Item2.linkedObject != null) continue;
            EXECUTE_JUMP(t.Item1, t.Item2);
        }
    }

    public static void EXECUTE_STAMPEDE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        EXECUTE_JUMP(caster, targetBlock);
        StampedeEvent se = new StampedeEvent("Stampede", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
        caster.addEvent(se);
        se.useIstantanely();
    }

    public static void EXECUTE_CAPERING(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        EXECUTE_JUMP(caster, targetBlock);
        Coordinate c = targetBlock.coordinate;
        List<Character> adj_heroes = ut_getAdjacentHeroes(c);
        foreach(Character adj in adj_heroes)
            adj.inflictDamage(calculateDamage(caster, adj, s));
    }

    public static void SWITCH_COWARD_MASK(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
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
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 35) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new ApathyEvent("Apathy", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_TORTUGA(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character c = targetBlock.linkedObject.GetComponent<Character>();
        TortugaEvent se = new TortugaEvent("Tortuga", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        c.addEvent(se);
        se.useIstantanely();
    }

    public static void EXECUTE_FURIA(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_comesCloser(caster, targetBlock, 2);
    }

    public static void EXECUTE_COMEDY(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_comesCloser(caster, targetBlock, 2);
        ut_repels(caster, targetBlock, 4);
    }

    public static void EXECUTE_APOSTASY(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 2);
    }

    public static void EXECUTE_PUDDLE_GLYPH(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getEnemies(caster)) {
            if (ut_isNearOf(caster, c, 4))
                c.addEvent(new PuddleGlyphEvent("Puddle Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
        }
    }

    public static void EXECUTE_AGGRESSIVE_GLYPH(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getEnemies(caster)) {
            if (ut_isNearOf(caster, c, 3))
                c.addEvent(new AggressiveGlyphEvent("Aggressive Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
        }
    }

    public static void EXECUTE_LIGHTNESS(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster))
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
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getEnemies(caster))
            if (ut_isNearOf(c, caster, 5))
                c.addEvent(new PerceptionGlyphEvent("Perception Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
    }

    public static void EXECUTE_BARRICADE(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        BarricadeEvent be = new BarricadeEvent("Barricade", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(be);
        be.useIstantanely();
    }

    public static void EXECUTE_REPULSION_GLYPH(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new RepulsionGlyphEvent("Repulsion Glyph", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_BURNING_GLYPH(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getEnemies(caster)) {
            if (ut_isNearOf(caster, c, 4))
                c.addEvent(new BurningGlyphEvent("Burning Glyph", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
        }
    }

    public static void EXECUTE_DAZZLING(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
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
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new FortificationEvent("Fortification", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_BONTAO(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster)) {
            c.receiveHeal(s.damage);
        }
    }

    public static void EXECUTE_TITANIC_HIT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_comesCloser(caster, targetBlock, 4);
    }

    public static void EXECUTE_TELLURIC_WAVE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        PolarityEvent pe = new PolarityEvent("Polarity", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(pe);
        pe.useIstantanely();
    }

    public static void EXECUTE_STRATEGA(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_repelsCaster(caster, targetBlock, 1);
        targetBlock.linkedObject.GetComponent<Character>().addEvent(
            new StrategaEvent("Stratega", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon)
        );
    }

    public static void EXECUTE_OVERCHARGE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        int remaining_pa = caster.getActualPA();
        if (remaining_pa > 0)
            caster.decrementPA(remaining_pa);
        int bonus_shield = remaining_pa * 40;
        caster.receiveShield(s.damage + bonus_shield);
    }

    public static void EXECUTE_STRIKING_METEOR(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        EXECUTE_JUMP(caster, targetBlock);
        foreach(Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(caster, enemy, 3))
                enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
    }

    public static void EXECUTE_AERIAL_WAVE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repelsCaster(caster, targetBlock, 3);
        ut_repels(caster, targetBlock, 3);
    }

    public static void EXECUTE_SELECTIVE_WORD(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        List<Character> heroes = ut_getAdjacentHeroes(targetBlock.coordinate);
        foreach (Character c in heroes)
            if (!c.isEnemyOf(caster))
                c.receiveHeal(30);
    }

    public static void EXECUTE_STRIKING_WORD(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Character ally in ut_getAllies(caster))
            if (ut_isNearOf(ally, targetBlock.linkedObject.GetComponent<Character>(), 3))
                ally.addEvent(new StrikingWordEvent("Striking Word", ally, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_RECOVERY_WORD(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new RecoveryWordEvent("Recovery Word", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        target.receiveHeal(target.hp - target.actual_hp);
    }

    public static void EXECUTE_PREVENTING_WORD(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        PreventingWordEvent pw = new PreventingWordEvent("Preventing Word", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(pw);
        pw.useIstantanely();
    }

    public static void EXECUTE_AGONISING_WORD(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        List<Character> adjs = ut_getAdjacentHeroes(targetBlock.coordinate);
        foreach(Character c in adjs) {
            c.inflictDamage(10);
            ut_repels(target, c.connectedCell.GetComponent<Block>(), 1);
        }
    }

    public static void EXECUTE_FURIOUS_WORD(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new FuriousWordEvent("Furious Word", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
        List<Character> adjs = ut_getAdjacentHeroes(targetBlock.coordinate);
        foreach (Character c in adjs)
            c.inflictDamage(50);
    }

    public static void EXECUTE_STIMULATING_WORD(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new StimulatingWordEvent("Stimulating Word", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_GALVANISING_WORD(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        List<Character> all = ut_getAllies(caster);
        all.AddRange(ut_getEnemies(caster));
        foreach (Character c in all)
            if (ut_isNearOf(c, caster, 3))
                c.addEvent(new GalvanisingWordEvent("Galvanising Word", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_PARALYSING_WORD(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new ParalysingWordEvent("Paralysing Word", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void SUMMONS_BWORK_MAGE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Bwork_Mage");
    }

    public static void SUMMONS_CRAQUELEUR(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Craqueleur");
    }

    public static void SUMMONS_GOBBALL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Gobball");
    }

    public static void SUMMONS_TOFU(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Tofu");
    }

    public static void SUMMONS_PRESPIC(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Prespic");
    }

    public static void SUMMONS_DRAGONNET(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Dragonnet");
    }

    public static void SUMMONS_PANDAWASTA(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Pandawasta");
    }

    public static void SUMMONS_BAMBOO(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Bamboo");
    }

    public static void SUMMONS_MADOLL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Madoll");
    }

    public static void SUMMONS_INFLATABLE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Inflatable");
    }

    public static void SUMMONS_SACRIFICIAL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Sacrificial_Doll");
    }

    public static void SUMMONS_THE_BLOCK(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "The_Block");
    }

    public static void SUMMONS_TREE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Tree");
    }

    public static void EXECUTE_STING(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        targetBlock.linkedObject.GetComponent<Character>().addEvent(new StingEvent("Sting", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_SEDIMENTATION(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        targetBlock.linkedObject.GetComponent<Character>().addEvent(new SedimentationEvent("Sedimentation", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_PLEASURE(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation)
            target.addEvent(new PleasureEvent("Pleasure", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_WHIP(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation)
            target.addEvent(new WhipEvent("Whip", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_SUMMONER_FURY(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation)
            target.addEvent(new SummonerFuryEvent("Summoner Fury", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_COMMUNION(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            CommunionEvent ce = new CommunionEvent("Communion", (Evocation)target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            target.addEvent(ce);
            ce.useIstantanely();
        }
    }

    public static void EXECUTE_CHAMRAK(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        EXECUTE_EXODUS(targetBlock.linkedObject.GetComponent<Character>(), caster.connectedCell.GetComponent<Block>(), s);
    }

    public static void EXECUTE_WATERFALL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        // Manual check to avoid ID check
        if (target is Evocation && target.name == "Bamboo" && target.team == caster.team) {
            foreach(Character c in ut_getAllies(caster)) {
                if (c.name != "Bamboo")
                    c.receiveHeal(20);
            }
        }
    }

    public static void EXECUTE_BLOW_OUT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        BlowOutEvent boe = new BlowOutEvent("Blow-Out", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
        caster.addEvent(boe);
        boe.useIstantanely();
    }

    public static void EXECUTE_EVICTION(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (caster.getEventSystem().getEvents("Blow-Out").Count > 0)
            ut_repels(caster, targetBlock, 5);
    }

    public static void EXECUTE_PANDJIU(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (caster.getEventSystem().getEvents("Blow-Out").Count > 0)
            ut_attracts(caster, targetBlock, 2);
    }
    public static void EXECUTE_EXPLOSIVE_FLASK(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (caster.getEventSystem().getEvents("Blow-Out").Count > 0)
            foreach (Character enemy in ut_getAllies(target))
                if (ut_isNearOf(target, enemy, 2))
                    enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
    }

    public static void EXECUTE_CONTAGION(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new ContagionEvent("Contagion", target, s.effectDuration, ParentEvent.Mode.ActivationEachEndTurn, s.icon, caster, s));
    }
    public static void EXECUTE_NATURE_POISON(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new NaturePoisonEvent("Nature Poison", target, s.effectDuration, ParentEvent.Mode.ActivationEachEndTurn, s.icon, caster, s));
    }

    public static void EXECUTE_EARTHQUAKE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(caster, enemy, 6)) {
                enemy.addEvent(new EarthquakeEvent("Earthquake", enemy, s.effectDuration, ParentEvent.Mode.ActivationEachEndTurn, s.icon, caster, s));
            }
    }
    public static void EXECUTE_DOLL_SACRIFICE(Character caster) {
        if (!put_CheckArguments(new System.Object[] { caster })) return;
        caster.inflictDamage(caster.actual_hp);
    }
    public static void EXECUTE_DOLL_SCREAM(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new DollScreamEvent("Doll Scream", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void SUMMONS_EXPLOBOMBE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Evocation bomb = ut_execute_summon(caster, targetBlock, "Explobombe");
        bomb.setBomb(caster, s);
    }

    public static void SUMMONS_TORNABOMBE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Evocation bomb = ut_execute_summon(caster, targetBlock, "Tornabombe");
        bomb.setBomb(caster, s);
    }

    public static void SUMMONS_WATERBOMBE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Evocation bomb = ut_execute_summon(caster, targetBlock, "Waterbombe");
        bomb.setBomb(caster, s);
    }

    public static void SUMMONS_LIVING_SHOVEL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Living Shovel");
    }

    // This method doesn't kill the bomb -> for security issues, do it manually
    public static void SUBEXECUTE_EXPLOSION(Evocation bomb, bool isSingleBomb) {
        if (!put_CheckArguments(new System.Object[] { bomb })) return;
        List<Character> allHeroes = ut_getEnemies(bomb);
        allHeroes.AddRange(ut_getAllies(bomb));
        if (!isSingleBomb)
            foreach (Character ch in allHeroes) {
                if (ut_isNearOf(ch, bomb, 2)) {
                    if (ch is Evocation) {
                        if (((Evocation)ch).isBomb && !bomb.connectedSummoner.summons.Contains((Evocation)ch)) {
                            ch.inflictDamage(bomb.getBombDamage(ch));
                            if (bomb.name == "Tornabombe")
                                ch.addEvent(new TornabombEvent("Tornabombe", ch, 1, ParentEvent.Mode.ActivationEachTurn, bomb.getBombSpellSprite()));
                            else if (bomb.name == "Waterbombe")
                                ch.addEvent(new WaterbombEvent("Waterbombe", ch, 1, ParentEvent.Mode.ActivationEachTurn, bomb.getBombSpellSprite()));
                        } else if (!((Evocation)ch).isBomb) {
                            ch.inflictDamage(bomb.getBombDamage(ch));
                            if (bomb.name == "Tornabombe")
                                ch.addEvent(new TornabombEvent("Tornabombe", ch, 1, ParentEvent.Mode.ActivationEachTurn, bomb.getBombSpellSprite()));
                            else if (bomb.name == "Waterbombe")
                                ch.addEvent(new WaterbombEvent("Waterbombe", ch, 1, ParentEvent.Mode.ActivationEachTurn, bomb.getBombSpellSprite()));
                        }
                        // don't execute damage on summoner bombs
                    } else {
                        ch.inflictDamage(bomb.getBombDamage(ch));
                        if (bomb.name == "Tornabombe")
                            ch.addEvent(new TornabombEvent("Tornabombe", ch, 1, ParentEvent.Mode.ActivationEachTurn, bomb.getBombSpellSprite()));
                        else if (bomb.name == "Waterbombe")
                            ch.addEvent(new WaterbombEvent("Waterbombe", ch, 1, ParentEvent.Mode.ActivationEachTurn, bomb.getBombSpellSprite()));
                    }
                }
            }
        else
        foreach (Character ch in allHeroes)
            if (ut_isNearOf(ch, bomb, 2)) {
                ch.inflictDamage(bomb.getBombDamage(ch));
                if (bomb.name == "Tornabombe")
                    ch.addEvent(new TornabombEvent("Tornabombe", ch, 1, ParentEvent.Mode.ActivationEachTurn, bomb.getBombSpellSprite()));
                else if (bomb.name == "Waterbombe")
                    ch.addEvent(new WaterbombEvent("Waterbombe", ch, 1, ParentEvent.Mode.ActivationEachTurn, bomb.getBombSpellSprite()));
            }
    }

    public static void EXECUTE_DETONATOR(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            Evocation evoTarget = (Evocation)target;
            if (evoTarget.isBomb && evoTarget.connectedSummoner.team == caster.team) {
                // the target is a bomb - every rogue ally can explode an ally bomb
                SUBEXECUTE_EXPLOSION(evoTarget, true);
                evoTarget.inflictDamage(evoTarget.actual_hp);
            }
        } else if (target.Equals(caster)) {
            // get all bombs from the caster and execute explosion
            List<Evocation> temp_summons = new List<Evocation>();
            foreach (Evocation evoTarget in caster.summons) {
                SUBEXECUTE_EXPLOSION(evoTarget, false);
                temp_summons.Add(evoTarget);
            }
            foreach (Evocation evoTarget in temp_summons) {
                evoTarget.inflictDamage(evoTarget.actual_hp);
            }
        }
    }

    public static void EXECUTE_POWDER(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            Evocation evoTarget = (Evocation)target;
            if (evoTarget.isBomb && evoTarget.connectedSummoner.team == caster.team) {
                evoTarget.setBombChargeToFive();
            }
        }
    }

    public static void EXECUTE_BOMB_TRICK(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            Evocation evoTarget = (Evocation)target;
            if (evoTarget.isBomb) {
	            EXECUTE_TRANSPOSITION(caster, targetBlock);
	            evoTarget.incrementBombCharge();
	            evoTarget.incrementBombCharge();
            }
        }
    }

    public static void EXECUTE_KICKBACK(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            Evocation evoTarget = (Evocation)target;
            if (evoTarget.isBomb) {
                // single target -> bomb
	            ut_repels(caster, targetBlock, 5);
	            evoTarget.incrementBombCharge();
            }
        } else if (target.Equals(caster)) {
            foreach (Character c in ut_getAdjacentHeroes(targetBlock.coordinate))
                if (c is Evocation)
	                if (((Evocation)c).isBomb) {
		                ut_repels(caster, c.connectedCell.GetComponent<Block>(), 3);
		                ((Evocation)c).incrementBombCharge();
	                }
        }
    }

    public static void EXECUTE_DECEPTION(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 20) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new DeceptionEvent("Deception", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void SUMMONS_SENTINEL_TURRECT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation turrect = ut_execute_summon(caster, targetBlock, "Sentinel_Turret");
        turrect.isTurrect = true;
    }

    public static void SUMMONS_GUARDIANA_TURRECT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation turrect = ut_execute_summon(caster, targetBlock, "Guardiana_Turret");
        turrect.isTurrect = true;
    }

    public static void SUMMONS_TACTICAL_TURRECT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation turrect = ut_execute_summon(caster, targetBlock, "Tactical_Turret");
        turrect.isTurrect = true;
    }

    public static void EXECUTE_HARPOONER_CHARGE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Evocation e in caster.summons)
            if (e.isTurrect)
                e.addEvent(new HarpoonerChargeEvent("Harpooner Charge", e, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_COMPASS(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        // No can teleport check
        Block casterBlock = caster.connectedCell.GetComponent<Block>();
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            if (!((Evocation)target).isTurrect) return;
        } else return;
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

    public static void EXECUTE_TRIDENT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character enemy = targetBlock.linkedObject.GetComponent<Character>();
        List<Character> otherEnemies = ut_getAllies(enemy);
        List<Character> toDamage = new List<Character>();
        if (otherEnemies.Count == 0) return;
        if (otherEnemies.Count == 1 || otherEnemies.Count == 2) toDamage.AddRange(otherEnemies);
        else {
            int first_index = -1, second_index = -1;
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            first_index = UnityEngine.Random.Range(0, otherEnemies.Count);
            while (second_index == -1 || second_index == first_index) {
                UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
                second_index = UnityEngine.Random.Range(0, otherEnemies.Count);
            }
            toDamage.Add(otherEnemies[first_index]);
            toDamage.Add(otherEnemies[second_index]);
        }
        if (toDamage.Count == 0) return;
        Character first = toDamage[0];
        Character second = null;
        if (toDamage.Count > 1) {
            second = toDamage[1];
        }
        first.inflictDamage((calculateDamage(caster, first, s)) / 2);
        if (second != null)
            second.inflictDamage((calculateDamage(caster, second, s)) / 2);
    }

    public static void EXECUTE_TORRENT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation && target.team == caster.team) {
            if (!((Evocation)target).isTurrect) return;
        } else return;
        target.addEvent(new TorrentEvent("Torrent", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_ASSISTANCE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation && target.team == caster.team) {
            if (!((Evocation)target).isTurrect) return;
        } else return;
        target.receiveHeal(target.hp - target.actual_hp);
    }

    public static void EXECUTE_REPULSION(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 5);
    }

    public static void EXECUTE_VIVACITY(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 3);
    }

    public static void SUMMONS_DOUBLE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation dd = ut_execute_summon(caster, targetBlock, "Double_" + caster.name);
        dd.isDouble = true;
    }

    public static void EXECUTE_MIST(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(enemy, caster, 3))
                enemy.addEvent(new MistEvent("Mist", enemy, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void SUMMONS_CHAFERFU(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (ut_getDeadStatsAllies(caster).Item1 > 1)
            ut_execute_summon(caster, targetBlock, "Chafer");
        else
            ut_execute_summon(caster, targetBlock, "Chafer_Lancer");
    }

    public static void EXECUTE_CRUELTY(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new CrueltyEvent("Cruelty", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, true));
        CrueltyEvent reservedCE = new CrueltyEvent("Cruelty", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, false);
        caster.addEvent(reservedCE);
        reservedCE.useIstantanely();
    }

    public static void EXECUTE_PERQUISITION(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character c in ut_getAllies(caster))
            if (ut_isNearOf(c, target, 2))
                c.receiveHeal(20);
    }

    public static void EXECUTE_EVASION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        int incrementPM = 0;
        foreach (Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(enemy, caster, 2))
                incrementPM += 2;
        if (incrementPM == 0) return;
        EvasionEvent evasion = new EvasionEvent("Evasion", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, incrementPM);
        caster.addEvent(evasion);
        evasion.useIstantanely();
    }

    public static void EXECUTE_CUT_THROAT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        CutThroatEvent cte = new CutThroatEvent("Cut Throat", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(cte);
        cte.useIstantanely();
    }

    public static void EXECUTE_TOXIC_INJECTION(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new ToxicInjectionEvent("Toxic Injection", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, s));
    }

    public static void EXECUTE_LARCENY(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character enemy = targetBlock.linkedObject.GetComponent<Character>();
        List<Character> otherEnemies = ut_getAllies(enemy);
        List<Character> toDamage = new List<Character>();
        if (otherEnemies.Count == 0 || ut_getDeadStatsAllies(caster).Item2 == 0) return;
        else if (otherEnemies.Count == 1 || otherEnemies.Count < ut_getDeadStatsAllies(caster).Item2) toDamage.AddRange(otherEnemies);
        else {
            List<int> chosenIndexes = new List<int>();
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int index = UnityEngine.Random.Range(0, otherEnemies.Count);
            chosenIndexes.Add(index);
            toDamage.Add(otherEnemies[index]);
            while (chosenIndexes.Count < ut_getDeadStatsAllies(caster).Item2) {
                UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
                index = UnityEngine.Random.Range(0, otherEnemies.Count);
                if (chosenIndexes.Contains(index)) continue;
                chosenIndexes.Add(index);
                toDamage.Add(otherEnemies[index]);
            }
        }
        if (toDamage.Count == 0) return;
        foreach (Character todmg in toDamage) {
            todmg.inflictDamage((calculateDamage(caster, todmg, s)) * 60 / 100);
        }
    }

    public static void EXECUTE_PULL_OUT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        PullOutEvent poe = new PullOutEvent("Pull Out", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
        caster.addEvent(poe);
        poe.useIstantanely();
    }

    public static void EXECUTE_TOOLBOX(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character c = targetBlock.linkedObject.GetComponent<Character>();
        c.addEvent(new ToolboxEvent("Toolbox", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        foreach(Evocation e in c.summons)
            e.addEvent(new ToolboxEvent("Toolbox", e, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_TUNNELING(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock ,s })) return;
        EXECUTE_JUMP(caster, targetBlock);
        foreach (Character enemy in ut_getAdjacentHeroes(targetBlock.coordinate))
            if (caster.isEnemyOf(enemy))
                enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
    }

    public static void EXECUTE_OBSOLESCENCE(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character c = targetBlock.linkedObject.GetComponent<Character>();
        c.addEvent(new ObsolescenceEvent("Obsolescence", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_FORTUNE(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character c = targetBlock.linkedObject.GetComponent<Character>();
        c.addEvent(new FortuneEvent("Fortune", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_MONEY_COLLECTION(Character caster) {
        if (!put_CheckArguments(new System.Object[] { caster })) return;
        caster.incrementKama(caster.actual_pa);
        caster.decrementPA(caster.actual_pa);
    }

    public static void EXECUTE_POWER_UNLOCKER(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        if (caster.getKama() < 10) return;
        PowerUnlockerEvent pue = null;
        if (caster.getKama() < 30) {
            pue = new PowerUnlockerEvent("Power Unlocker", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon, 1);
            caster.decrementKama(10);
        } else {
            pue = new PowerUnlockerEvent("Power Unlocker", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon, 2);
            caster.decrementKama(30);
        }
        caster.addEvent(pue);
        pue.useIstantanely();
    }

    #endregion

    #region MONSTER SPELLS SPECIALIZATION

    public static void EXECUTE_BRIKOCOOP(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster))
            c.receiveHeal(s.damage);
    }

    public static void EXECUTE_BRIKOASSAULT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!caster.canMovedByEffects) return;
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
            Block toJump = Map.Instance.getBlock(new Coordinate(targetCoord.row - 1, targetCoord.column));
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
        caster.setZIndex(caster.connectedCell.GetComponent<Block>());
    }

    public static void EXECUTE_BRIKO_STIMULATION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster))
            c.addEvent(new BrikoStimulationEvent("Briko Stimulation", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_BRUTO_STIMULATION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster))
            c.addEvent(new BrutoStimulationEvent("Bruto Stimulation", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_MANIFOLD_BRAMBLE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character c in ut_getAllies(target)) {
            if (ut_isNearOf(target, c, 2)) {
                c.inflictDamage(Spell.calculateDamage(caster, c, s));
            }
        }
    }

    #endregion

    #region EVENT BONUSES

    public static int BONUS_ACCUMULATION = 4;
    public static int BONUS_WRATH = 120;
    public static int BONUS_BOW_SKILL = 12;
    public static int BONUS_ATONEMENT_ARROW = 36;
    public static int BONUS_DECIMATION = 48;
    public static int BONUS_SHADOWYBEAM = 13;
    public static int BONUS_LETHAL_ATTACK = 30;
    public static int BONUS_KAMA_THROWING = 18;

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
        } else if (caster.name == "Etraggy" && s.name == "Lethal Attack") {
            if (ut_getDeadStatsAllies(caster).Item1 == 1) return BONUS_LETHAL_ATTACK;
            else return 0;
        } else if (caster.name == "Diver Birel" && s.name == "Kama Throwing") {
            if (caster.getKama() > 0) {
                caster.decrementKama(1);
                return BONUS_KAMA_THROWING;
            } else return 0;
        } else return 0;
    }

    #endregion

    #region UTILITIES

    // Tuple<ALIVE, DEAD>
    public static Tuple<int, int> ut_getDeadStatsAllies(Character caster) {
        if (!put_CheckArguments(new System.Object[] { caster })) return null;
        int counterDead = 0;
        int counterAlive = 0;
        foreach(Character c in TurnsManager.Instance.allCharacters) {
            if (!c.isEnemyOf(caster) && c.isDead) counterDead++;
            else if (!c.isEnemyOf(caster) && !c.isDead) counterAlive++;
        }
        return new Tuple<int, int>(counterAlive, counterDead);
    }

    public static Evocation ut_execute_summon(Character caster, Block targetBlock, string id) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return null;
        GameObject summonPrefab = Resources.Load("Prefabs/Heroes/Evocations/" + id) as GameObject;
        // Creating summon
        GameObject summon = GameObject.Instantiate(summonPrefab, Coordinate.getPosition(targetBlock.coordinate), Quaternion.identity);
        // Placing it on field
        summon.transform.position = new Vector3(summon.transform.position.x, summon.transform.position.y, -20);
        targetBlock.linkedObject = summon;
        PreparationManager.Instance.setStandManually(summon, caster.team);
        // Setting summon parameters
        Evocation summonScript = summon.GetComponent<Evocation>();
        summonScript.isEvocation = true;
        summonScript.id = caster.summonsIdCounter;
        caster.summonsIdCounter++;
        summonScript.team = caster.team;
        summonScript.connectedSummoner = caster;
        summonScript.connectedCell = targetBlock.gameObject;
        summonScript.setZIndex(targetBlock);
        caster.summons.Add(summonScript);
        // Setting turns parameters
        TurnsManager.Instance.injectCharacter(caster, summonScript);
        return summonScript;
    }

    public static List<Character> ut_getAllies(Character caster) {
        if (!put_CheckArguments(new System.Object[] { caster })) return null;
        List<Character> toReturn = new List<Character>();
        foreach (Character ch in TurnsManager.Instance.turns) {
            if (ch.isDead) continue;
            if (ch.team == caster.team && !ch.EqualsNames(caster)) {
                toReturn.Add(ch);
            }
        }
        return toReturn;
    }

    public static List<Character> ut_getAlliesWithCaster(Character caster) {
        if (!put_CheckArguments(new System.Object[] { caster })) return null;
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
        if (!put_CheckArguments(new System.Object[] { caster })) return null;
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
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (!targetBlock.linkedObject.GetComponent<Character>().canMovedByEffects) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!caster.canMovedByEffects) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (!targetBlock.linkedObject.GetComponent<Character>().canMovedByEffects) return;
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
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!caster.canMovedByEffects) return;
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
        if (!put_CheckArguments(new System.Object[] { a, b })) return false;
        int dist_row = Mathf.Abs(a.row - b.row);
        int dist_col = Mathf.Abs(a.column - b.column);
        return (dist_row + dist_col <= cells);
    }

    public static bool ut_isNearOf(Character ac, Character bc, int cells) {
        if (!put_CheckArguments(new System.Object[] { ac, bc })) return false;
        Coordinate a = ac.connectedCell.GetComponent<Block>().coordinate;
        Coordinate b = bc.connectedCell.GetComponent<Block>().coordinate;
        int dist_row = Mathf.Abs(a.row - b.row);
        int dist_col = Mathf.Abs(a.column - b.column);
        return (dist_row + dist_col <= cells);
    }

    public static List<Character> ut_getAdjacentHeroes(Coordinate c) {
        if (!put_CheckArguments(new System.Object[] { c })) return new List<Character>();
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

    public static bool put_CheckArguments(System.Object[] args) {
        bool toRet = true;
        for (int i = 0; i < args.Length; i++)
            if (args[i] == null) {
                toRet = false;
                break;
            } else {
                if (args[i] is Character)
                    if (((Character)args[i]).isDead) {
                        toRet = false;
                        break;
                    }
            }
        return toRet;
    }

    public static bool put_CheckLinkedObject(Block b) {
        if (b.linkedObject == null) return false;
        return true;
    }

    #endregion
}