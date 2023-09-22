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
        if (caster is Character && !(caster is Monster)) {
            if (spell.isSummon && caster.summons.Count == caster.numberOfSummons) return false;
        }
        if (caster is Monster) {
            if (spell.isSummon && caster.monsterSummons.Count == caster.numberOfSummons) return false;
        }
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
        if (spell.name != "Shushu Cut" && caster.name == "Tristepin") {
            if (caster.getEventSystem().getEvents("Yop God Status").Count == 0) {
                caster.shushuCounter = 0;
                Debug.Log("Shushu counter restored");
            }
        }
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
        int finalDamage = (damage + (damage * bonus_attack / 100));
        finalDamage -= (finalDamage * resistance / 100);
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        if (ut_isNearOf(caster, target, 3) && target.immuneCloseCombat)
            finalDamage = 0;
        if (finalDamage > 0) {
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
        }
        return finalDamage;
    }

    public static void executeSpell(Character caster, Block targetBlock, Spell spell) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, spell })) return;
        if (!canUse(caster, spell)) return;
        if (targetBlock.linkedObject == null) {
            // no target
            if (spell.canUseInEmptyCell) {
                payCost(caster, spell);
                if (spell.hasEffect)
                {
                    // SPECIALIZATIONS HERE
                    if (!(caster is Monster))
                        SPELL_SPECIALIZATION(caster, targetBlock, spell);
                    else
                        MONSTER_SPELL_SPECIALIZATION(caster, targetBlock, spell);
                }
                // Code here - Spells in empty cells
            } else return;
        } else {
            if (spell.canUseInEmptyCell)
                return;
            payCost(caster, spell);
            int damageToInflict = 0;
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            if (spell.damage > 0)
                damageToInflict = calculateDamage(caster, target, spell);
            if (spell.hasEffect)
            {
                // SPECIALIZATIONS HERE
                if (!(caster is Monster))
                    SPELL_SPECIALIZATION(caster, targetBlock, spell);
                else
                    MONSTER_SPELL_SPECIALIZATION(caster, targetBlock, spell);
            }
            // Code here - Spells on target
            if (!spell.isEffectOnly && spell.damage > 0) {
                if (spell.element != Element.Heal) {
                    if (target.connectedSacrifice == null)
                        target.inflictDamage(damageToInflict);
                    else target.connectedSacrifice.inflictDamage(damageToInflict);
                } else target.receiveHeal(damageToInflict + caster.bonusHeal);
                if (spell.lifeSteal) caster.receiveHeal(damageToInflict / 2);
            }
            if (caster.name == "Hellraiser" && target.name == "Tactical Beacon") {
                List<Character> enemies = Spell.ut_getEnemies(caster);
                foreach (Character c in enemies) {
                    if (Spell.ut_isNearOf(c, target, 3))
                        c.inflictDamage(damageToInflict * 40 / 100);
                }
            }
        }
        caster.addSpell(spell);
    }

    public bool isOffensiveSpell() {
        return (this.element == Element.Air || this.element == Element.Fire || this.element == Element.Earth || this.element == Element.Water);
    }

    #region SPELL SPECIALIZATIONS

    public static void SPELL_SPECIALIZATION(Character caster, Block targetBlock, Spell spell) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, spell })) return;
        if (spell.name == "Jump" || spell.name == "Catnip") EXECUTE_JUMP(caster, targetBlock);
        else if (spell.name == "Pounding") EXECUTE_POUNDING(targetBlock, spell);
        else if (spell.name == "Agitation") EXECUTE_AGITATION(targetBlock, spell);
        else if (spell.name == "Accumulation") EXECUTE_ACCUMULATION(caster, spell);
        else if (spell.name == "Power") EXECUTE_POWER(targetBlock, spell);
        else if (spell.name == "Inferno") EXECUTE_INFERNO(caster, targetBlock, spell);
        else if (spell.name == "Duel") EXECUTE_DUEL(caster, targetBlock, spell);
        else if (spell.name == "Iop's Wrath") EXECUTE_IOP_WRATH(caster, spell);
        else if (spell.name == "Extrasensory Perception") EXECUTE_EXTRASENSORY_PERCEPTION(caster, spell);
        else if (spell.name == "Indomitable Will") EXECUTE_INDOMITABLE_WILL(caster, spell);
        else if (spell.name == "Devastate" || spell.name == "Crazy Cutting") EXECUTE_DEVASTATE(caster, spell);
        else if (spell.name == "Sentence") EXECUTE_SENTENCE(caster, spell);
        else if (spell.name == "Fight Back") EXECUTE_FIGHT_BACK(caster, spell);
        else if (spell.name == "Compulsion") EXECUTE_COMPULSION(targetBlock, spell);
        else if (spell.name == "Stretching") EXECUTE_STRETCHING(caster, spell);
        else if (spell.name == "Composure") EXECUTE_COMPOSURE(targetBlock, spell);
        else if (spell.name == "Virus") EXECUTE_VIRUS(caster, spell, targetBlock);
        else if (spell.name == "Karcham") EXECUTE_KARCHAM(caster, targetBlock, spell);
        else if (spell.name == "Sword of Yop") EXECUTE_SWORD_OF_YOP(caster, targetBlock, spell);
        else if (spell.name == "Tumult") EXECUTE_TUMULT(caster, targetBlock, spell);
        else if (spell.name == "Powerful Shooting") EXECUTE_POWERFUL_SHOOTING(targetBlock, spell);
        else if (spell.name == "Bow Skill") EXECUTE_BOW_SKILL(caster, spell);
        else if (spell.name == "Slow Down Arrow") EXECUTE_SLOW_DOWN_ARROW(targetBlock, spell);
        else if (spell.name == "Atonement Arrow") EXECUTE_ATONEMENT_ARROW(caster, spell);
        else if (spell.name == "Retreat Arrow" || spell.name == "Tricky Blow") EXECUTE_RETREAT_ARROW(caster, targetBlock);
        else if (spell.name == "Barricade Shot") EXECUTE_BARRICADE_SHOT(caster, targetBlock, spell);
        else if (spell.name == "Sentinel") EXECUTE_SENTINEL(caster, spell);
        else if (spell.name == "Critical Shooting") EXECUTE_CRITICAL_SHOOTING(targetBlock, spell);
        else if (spell.name == "Contempt" || spell.name == "Exodus" || spell.name == "Feline Spirit" || spell.name == "Pivot") EXECUTE_EXODUS(caster, targetBlock, spell);
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
        else if (spell.name == "Heads or Tails") EXECUTE_HEADS_OR_TAILS(caster, targetBlock, spell);
        else if (spell.name == "All or Nothing") EXECUTE_ALL_OR_NOTHING(caster, targetBlock, spell);
        else if (spell.name == "Claw of Ceangal" || spell.name == "Haunting Magic") EXECUTE_CLAW_OF_CEANGAL(caster, targetBlock);
        else if (spell.name == "Godsend") EXECUTE_GODSEND(targetBlock, spell);
        else if (spell.name == "Feline Sense") EXECUTE_FELINE_SENSE(caster, targetBlock, spell);
        else if (spell.name == "Roulette") EXECUTE_ROULETTE(caster, spell);
        else if (spell.name == "Time Rift") EXECUTE_TIME_RIFT(caster, targetBlock, spell);
        else if (spell.name == "Sandglass") EXECUTE_SANDGLASS(targetBlock, spell);
        else if (spell.name == "Rewind") EXECUTE_REWIND(targetBlock, spell);
        else if (spell.name == "Clock") EXECUTE_CLOCK(targetBlock, spell);
        else if (spell.name == "Time Theft") EXECUTE_TIME_THEFT(caster, targetBlock, spell);
        else if (spell.name == "Boliche") EXECUTE_BOLICHE(caster, targetBlock);
        else if (spell.name == "Trance") EXECUTE_TRANCE(caster, targetBlock);
        else if (spell.name == "Stunt") EXECUTE_STUNT(caster, targetBlock, spell);
        else if (spell.name == "Haziness") EXECUTE_HAZINESS(targetBlock, spell);
        else if (spell.name == "Slow Down") EXECUTE_SLOW_DOWN(targetBlock, spell);
        else if (spell.name == "Gear") EXECUTE_GEAR(targetBlock, spell);
        else if (spell.name == "Restart") EXECUTE_RESTART(spell);
        else if (spell.name == "Stampede") EXECUTE_STAMPEDE(caster, targetBlock, spell);
        else if (spell.name == "Shelly Placement") SUMMONS_SHELLY(caster, targetBlock);
        else if (spell.name == "Scaphander") SUMMONS_SCAPHANDER(caster, targetBlock);
        else if (spell.name == "Sonar") EXECUTE_SONAR(caster, spell);
        else if (spell.name == "Withdrawal Arrow") EXECUTE_WITHDRAWAL_ARROW(caster, targetBlock, spell);
        else if (spell.name == "Repulsive Arrow") EXECUTE_REPULSIVE_ARROW(caster, targetBlock, spell);
        else if (spell.name == "Fulminating Arrow") EXECUTE_FULMINATING_ARROW(caster, targetBlock, spell);
        else if (spell.name == "Capering") EXECUTE_CAPERING(caster, targetBlock, spell);
        else if (spell.name == "Coward Mask") SWITCH_COWARD_MASK(caster, spell);
        else if (spell.name == "Psychopath Mask") SWITCH_PSYCHOPATH_MASK(caster, spell);
        else if (spell.name == "Tireless Mask") SWITCH_TIRELESS_MASK(caster, spell);
        else if (spell.name == "Tortuga") EXECUTE_TORTUGA(caster, targetBlock, spell);
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
        else if (spell.name == "Fortification") EXECUTE_FORTIFICATION(caster, targetBlock, spell);
        else if (spell.name == "Prey") EXECUTE_PREY(targetBlock, spell);
        else if (spell.name == "Furious Hunting") EXECUTE_FURIOUS_HUNTING(caster, targetBlock, spell);
        else if (spell.name == "Bloodhound") EXECUTE_BLOODHOUND(caster, targetBlock, spell);
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
        else if (spell.name == "Voracious Sword") SUMMONS_FLYING_SWORD(caster, targetBlock);
        else if (spell.name == "Call of Craqueleur") SUMMONS_CRAQUELEUR(caster, targetBlock);
        else if (spell.name == "Call of Dragonnet") SUMMONS_DRAGONNET(caster, targetBlock);
        else if (spell.name == "Call of Tofu") SUMMONS_TOFU(caster, targetBlock);
        else if (spell.name == "Xelor Dial") XELOR_DIAL(caster, targetBlock);
        else if (spell.name == "Call of Gobball") SUMMONS_GOBBALL(caster, targetBlock);
        else if (spell.name == "Elemental Resurrection") SUMMONS_ELEMENTAL_GUARDIAN(caster, targetBlock);
        else if (spell.name == "Creation") SUMMONS_RUNE(caster, targetBlock);
        else if (spell.name == "Elemental Drain") EXECUTE_ELEMENTAL_DRAIN(caster, spell);
        else if (spell.name == "Runic Overcharge") EXECUTE_RUNIC_OVERCHARGE(targetBlock, spell);
        else if (spell.name == "Mass Grave") EXECUTE_MASS_GRAVE(caster, targetBlock);
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
        else if (spell.name == "Hupperpunch" || spell.name == "Hupperkut") EXECUTE_HUPPERDIE(caster, targetBlock, spell);
        else if (spell.name == "Death Con") EXECUTE_DEATH_CON(caster, targetBlock, spell);
        else if (spell.name == "Sun Lance") EXECUTE_SUN_LANCE(caster, targetBlock, spell);
        else if (spell.name == "Contagion") EXECUTE_CONTAGION(caster, targetBlock, spell);
        else if (spell.name == "Nature Poison") EXECUTE_NATURE_POISON(caster, targetBlock, spell);
        else if (spell.name == "Earthquake") EXECUTE_EARTHQUAKE(caster, spell);
        else if (spell.name == "Perfidy") EXECUTE_PERFIDY(caster, spell);
        else if (spell.name == "Doll Sacrifice") EXECUTE_DOLL_SACRIFICE(caster, targetBlock, spell);
        else if (spell.name == "Temporal Paradox") EXECUTE_TEMPORAL_PARADOX(caster, targetBlock, spell);
        else if (spell.name == "Overclock") EXECUTE_OVERCLOCK(caster, targetBlock, spell);
        else if (spell.name == "Petrification") EXECUTE_PETRIFICATION(caster, targetBlock, spell);
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
        else if (spell.name == "Tactical Beacon") SUMMONS_TACTICAL_BEACON(caster, targetBlock);
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
        else if (spell.name == "Tailing") EXECUTE_TAILING(caster, targetBlock);
        else if (spell.name == "Vertebra") EXECUTE_VERTEBRA(caster, targetBlock, spell);
        else if (spell.name == "Obsolescence") EXECUTE_OBSOLESCENCE(targetBlock, spell);
        else if (spell.name == "Fortune") EXECUTE_FORTUNE(targetBlock, spell);
        else if (spell.name == "Money Collection") EXECUTE_MONEY_COLLECTION(caster);
        else if (spell.name == "Power Unlocker") EXECUTE_POWER_UNLOCKER(caster, spell);
        else if (spell.name == "Smithereens") EXECUTE_SMITHEREENS(caster, spell);
        else if (spell.name == "Watchdog" || spell.name == "Beaten" || spell.name == "Tetanisation" || spell.name == "Calcaneus") ADD_RAGE_COUNTER(caster, 1);
        else if (spell.name == "Pursuit") EXECUTE_PURSUIT(caster, spell);
        else if (spell.name == "Bark") EXECUTE_BARK(targetBlock, spell);
        else if (spell.name == "Cerberus") EXECUTE_CERBERUS(caster, targetBlock, spell);
        else if (spell.name == "Appeasement") EXECUTE_APPEASEMENT(caster, targetBlock, spell);
        else if (spell.name == "Jaw") EXECUTE_JAW(caster, targetBlock, spell);
        else if (spell.name == "Pawerful") EXECUTE_PAWERFUL(caster, spell);
        else if (spell.name == "Fracture") EXECUTE_FRACTURE(caster, targetBlock, spell);
        else if (spell.name == "Destructive Ring") EXECUTE_DESTRUCTIVE_RING(caster, targetBlock, spell);
        else if (spell.name == "Wakmeha") EXECUTE_WAKMEHA(caster, targetBlock, spell);
        else if (spell.name == "Redemption Arrow") EXECUTE_REDEMPTION_ARROW(caster, targetBlock, spell);
        else if (spell.name == "Audacious") EXECUTE_AUDACIOUS(caster, targetBlock, spell);
        else if (spell.name == "Affront") EXECUTE_AFFRONT(caster, targetBlock, spell);
        else if (spell.name == "Lightning Fist") EXECUTE_LIGHTNING_FIST(caster, targetBlock, spell);
        else if (spell.name == "Explosive Arrow") EXECUTE_EXPLOSIVE_ARROW(caster, targetBlock, spell);
        else if (spell.name == "Celestial Sword") EXECUTE_CELESTIAL_SWORD(caster, targetBlock, spell);
        else if (spell.name == "Musket") EXECUTE_MUSKET(caster, targetBlock, spell);
        else if (spell.name == "Bombard") EXECUTE_BOMBARD(caster, targetBlock, spell);
        else if (spell.name == "Cadence") EXECUTE_CADENCE(caster, targetBlock, spell);
        else if (spell.name == "Bomb Strategy") EXECUTE_BOMB_STRATEGY(caster, spell);
        else if (spell.name == "Sword of Fate") EXECUTE_SWORD_OF_FATE(caster, spell);
        else if (spell.name == "Violence") EXECUTE_VIOLENCE(caster, targetBlock, spell);
        else if (spell.name == "Neutral") EXECUTE_NEUTRAL(caster, targetBlock, spell);
        else if (spell.name == "Bloody Hostility") EXECUTE_BLOODY_HOSTILITY(caster, spell);
        else if (spell.name == "Bloody Punishment") EXECUTE_BLOODY_PUNISHMENT(caster, spell);
        else if (spell.name == "Reprisal") EXECUTE_REPRISAL(caster, targetBlock);
        else if (spell.name == "Ronda") EXECUTE_CONTEMPT(caster, targetBlock);
        else if (spell.name == "Scudo") EXECUTE_SCUDO(caster, spell);
        else if (spell.name == "Immobilising Arrow") EXECUTE_IMMOBILISING_ARROW(targetBlock, spell);
        else if (spell.name == "Destructive Arrow") EXECUTE_DESTRUCTIVE_ARROW(targetBlock, spell);
        else if (spell.name == "Assailing Arrow") EXECUTE_ASSAILING_ARROW(caster, spell);
        else if (spell.name == "Punitive Arrow") EXECUTE_PUNITIVE_ARROW(caster, spell);
        else if (spell.name == "Friendly Armor") EXECUTE_FRIENDLY_ARMOR(caster, targetBlock, spell);
        else if (spell.name == "Truce") EXECUTE_TRUCE(caster, targetBlock, spell);
        else if (spell.name == "Bubble") EXECUTE_BUBBLE(caster, targetBlock, spell);
        else if (spell.name == "Natural Attraction") EXECUTE_NATURAL_ATTRACTION(caster, targetBlock);
        else if (spell.name == "Feca Protector") SUMMONS_AEGIS(caster, targetBlock);
        else if (spell.name == "Aegis Armor") EXECUTE_AEGIS_ARMOR(caster, spell);
        else if (spell.name == "Team Word") EXECUTE_TEAM_WORD(caster, targetBlock, spell);
        else if (spell.name == "Helping Word") EXECUTE_HELPING_WORD(caster, targetBlock, spell);
        else if (spell.name == "Friendship Word") SUMMONS_CUNEY(caster, targetBlock);
        else if (spell.name == "Pandawa Pint") SUMMONS_PINT(caster, targetBlock);
        else if (spell.name == "Frightening Word") EXECUTE_FRIGHTENING_WORD(caster, targetBlock);
        else if (spell.name == "Cat Soul") EXECUTE_CAT_SOUL(caster, targetBlock, spell);
        else if (spell.name == "Cat Assault") EXECUTE_CAT_ASSAULT(caster, targetBlock, spell);
        else if (spell.name == "Summoning Claw") SUMMONS_CLAW(caster, targetBlock);
        else if (spell.name == "Portal") SUMMONS_PORTAL(caster, targetBlock, false);
        else if (spell.name == "Flexible Portal") SUMMONS_PORTAL(caster, targetBlock, true);
        else if (spell.name == "Portal Coalition") EXECUTE_PORTAL_COALITION(caster, targetBlock, spell);
        else if (spell.name == "Offence") EXECUTE_OFFENCE(caster, targetBlock, spell);
        else if (spell.name == "Sinecure") EXECUTE_SINECURE(caster, targetBlock, spell);
        else if (spell.name == "Portal Interruption") EXECUTE_PORTAL_INTERRUPTION(caster);
        else if (spell.name == "Unconscious Combat") EXECUTE_UNCONSCIOUS_COMBAT(caster, spell);
        else if (spell.name == "Reflex") EXECUTE_REFLEX(caster, spell);
        else if (spell.name == "Thunderclap and Flash") EXECUTE_THUNDERCLAP_FLASH(caster, targetBlock, spell);
        else if (spell.name == "Water Wheel") EXECUTE_WATER_WHEEL(caster, targetBlock, spell);
        else if (spell.name == "Flowing Dance") EXECUTE_FLOWING_DANCE(caster, targetBlock, spell);
        else if (spell.name == "Raging Sun") EXECUTE_RAGING_SUN(caster, targetBlock, spell);
        else if (spell.name == "Burning Bones") EXECUTE_BURNING_BONES(caster, targetBlock, spell);
        else if (spell.name == "Godlike Speed") EXECUTE_GODLIKE_SPEED(caster, targetBlock);
        else if (spell.name == "Hellbak") EXECUTE_HELLBAK(caster, targetBlock, spell);
        else if (spell.name == "Changebak") EXECUTE_CHANGEBAK(caster, targetBlock, spell);
        else if (spell.name == "Shootinbak") EXECUTE_SHOOTINBAK(caster, targetBlock, spell);

        // ADD HERE ELSE IF (...) ...
        else Debug.LogError("Effect for " + spell.name + " has not implemented yet");
    }

    public static void MONSTER_SPELL_SPECIALIZATION(Character caster, Block targetBlock, Spell spell) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, spell })) return;
        if (spell.name == "Brikocoop" || spell.name == "Brutocoop") EXECUTE_BRIKOCOOP(caster, spell);
        else if (spell.name == "Briko Assault" || spell.name == "Bruto Assault" || spell.name == "Royal Crushing" || spell.name == "Croc Assault") EXECUTE_BRIKOASSAULT(caster, targetBlock, spell);
        else if (spell.name == "Briko Stimulation") EXECUTE_BRIKO_STIMULATION(caster, spell);
        else if (spell.name == "Sting") EXECUTE_STING(targetBlock, spell);
        else if (spell.name == "Wild Lash" || spell.name == "Breeze") EXECUTE_RETREAT_ARROW(caster, targetBlock);
        else if (spell.name == "Manifold Bramble") EXECUTE_MANIFOLD_BRAMBLE(caster, targetBlock, spell);
        else if (spell.name == "Bruto Stimulation") EXECUTE_BRUTO_STIMULATION(caster, spell);
        else if (spell.name == "Explosive Egg") EXECUTE_EXPLOSIVE_EGG(caster, targetBlock, spell);
        else if (spell.name == "My Tofu Childs") EXECUTE_MY_TOFU_CHILDS(caster, targetBlock, spell);
        else if (spell.name == "Attractive Smell") EXECUTE_ATTRACTIVE_SMELL(caster, targetBlock, spell);
        else if (spell.name == "Call of the forest") EXECUTE_CALL_OF_THE_FOREST(caster, targetBlock, spell);
        else if (spell.name == "Call of the cat") EXECUTE_CALL_OF_THE_CAT(caster, targetBlock, spell);
        else if (spell.name == "Meka Evocation") EXECUTE_CALL_OF_THE_MEKA(caster, targetBlock, spell);
        else if (spell.name == "Kannilance") EXECUTE_KANNILANCE(caster, targetBlock, spell);
        else if (spell.name == "Wolf Cry") EXECUTE_WOLF_CRY(caster, spell);
        else if (spell.name == "Vilinslash") EXECUTE_VILINSLASH(caster, targetBlock, spell);
        else if (spell.name == "Wax Shot") EXECUTE_WAX_SHOT(caster, targetBlock, spell);
        else if (spell.name == "Evocation killer") EXECUTE_EVOKILLER(caster, targetBlock, spell);
        else if (spell.name == "Ninjawax") EXECUTE_NINJAWAX(caster, targetBlock, spell);
        else if (spell.name == "Kankenswift") EXECUTE_KANKENSWIFT(caster, targetBlock);
        else if (spell.name == "Kankendust") EXECUTE_KANKENDUST(caster, targetBlock, spell);
        else if (spell.name == "Mud throw") EXECUTE_MUD_THROW(caster, targetBlock, spell);
        else if (spell.name == "Mud shot") EXECUTE_MUD_SHOT(caster, targetBlock);
        else if (spell.name == "Insect Cry") EXECUTE_INSECT_CRY(caster, targetBlock, spell);
        else if (spell.name == "Devil Hungry") EXECUTE_DEVIL_HUNGRY(caster, spell);
        else if (spell.name == "Hideout") EXECUTE_HIDEOUT(caster, spell);
        else if (spell.name == "Healing plant") EXECUTE_HEALING_PLANT(caster, spell);
        else if (spell.name == "Hard Bone") EXECUTE_HARD_BONE(caster, spell);
        else if (spell.name == "Ax of the Valkyrie" || spell.name == "Ax of Bworkette" || spell.name == "Double cut") EXECUTE_AX_OF_THE_VALKYRIE(caster, targetBlock, spell);
        else if (spell.name == "Destruction Sword") EXECUTE_DESTRUCTION_SWORD(caster, targetBlock, spell);
        else if (spell.name == "Fate of light") EXECUTE_FATE_OF_LIGHT(caster, spell);
        else if (spell.name == "Lights out") EXECUTE_LIGHTS_OUT(caster, spell);
        else if (spell.name == "Psycho Analysis") EXECUTE_PSYCHO_ANALYSIS(caster, spell);
        else if (spell.name == "Birth") EXECUTE_BIRTH(caster, targetBlock, spell);
        else if (spell.name == "Poisoned Fog") EXECUTE_POISONED_FOG(caster, targetBlock, spell);
        else if (spell.name == "Holy Chafer Sword" || spell.name == "Orat hit") EXECUTE_HOLY_CHAFER_SWORD(caster, targetBlock, spell);
        else if (spell.name == "Chafer Fireshot") EXECUTE_CHAFER_FIRESHOT(caster, targetBlock, spell);
        else if (spell.name == "Chafer Windshot") EXECUTE_CHAFER_WINDSHOT(caster, targetBlock);
        else if (spell.name == "Chafer Lance Explosion") EXECUTE_CHAFER_LANCE_EXPLOSION(caster, spell);
        else if (spell.name == "Kwablow" || spell.name == "Skewering") EXECUTE_REPULSION(caster, targetBlock);
        else if (spell.name == "Kwasmutation") EXECUTE_KWASMUTATION(caster, spell);
        else if (spell.name == "Magic Power") EXECUTE_MAGIC_POWER(caster, spell);
        else if (spell.name == "Shootem Bombe") SUMMONS_ROGUEBOMB_DISTANCE(caster, targetBlock, spell);
        else if (spell.name == "Surrounded") SUMMONS_ROGUEBOMB_EVERYFREE(caster, targetBlock, spell);
        else if (spell.name == "Bomberman") EXECUTE_BOMBERMAN(caster, targetBlock);
        else if (spell.name == "Junior Bwork Distance") EXECUTE_JUNIOR_BWORK_DISTANCE(caster, targetBlock);
        else if (spell.name == "Arachnee Population") EXECUTE_ARACHNEE_POPULATION(caster, targetBlock, spell);
        else if (spell.name == "Call of Arachnee") EXECUTE_CALL_OF_ARACHNEE(caster, targetBlock, spell);
        else if (spell.name == "Alpha Teeth") EXECUTE_ALPHA_TEETH(caster, targetBlock, spell);
        else if (spell.name == "Crobak Shot") SUMMONS_CROBAK_DISTANCE(caster, targetBlock, spell);
        else if (spell.name == "Crobak Liberation") EXECUTE_MY_CROBAK_CHILDS(caster, targetBlock, spell);
        else if (spell.name == "Blocking Spit") EXECUTE_BLOCKING_SPIT(caster, targetBlock, spell);
        else if (spell.name == "High Temperature") EXECUTE_HIGH_TEMPERATURE(caster, targetBlock, spell);
        else if (spell.name == "Gwandisolation") EXECUTE_GWANDISOLATION(caster, targetBlock);
        else if (spell.name == "Bottaboule") EXECUTE_BOTTABOULE(caster, targetBlock, spell);
        else if (spell.name == "Call of Biblop") EXECUTE_CALL_OF_BIBLOP(caster, targetBlock, spell);
        else if (spell.name == "Bloppy Hungry") EXECUTE_BLOPPY_HUNGRY(caster);
        else if (spell.name == "Gwandhit") EXECUTE_GWANDHIT(caster, targetBlock, spell);
        else if (spell.name == "Sedimentation") EXECUTE_SEDIMENTATION(targetBlock, spell);
        else if (spell.name == "Legendaire Punch") EXECUTE_LEGENDAIRE_PUNCH(caster, spell);
        else if (spell.name == "Group Wabbheal") EXECUTE_GROUP_WABBHEAL(caster, spell);
        else if (spell.name == "Katana Lunge") EXECUTE_KATANA_LUNGE(caster, targetBlock, spell);
        else if (spell.name == "Kannidestruction") EXECUTE_KANNIDESTRUCTION(caster, spell);
        else if (spell.name == "Turtle Hypermove") EXECUTE_TURTLE_HYPERMOVE(caster, spell);
        else if (spell.name == "Turtle Catch") EXECUTE_TURTLE_CATCH(caster, targetBlock);
        else if (spell.name == "Long Turtle Hit") EXECUTE_LONG_TURTLE_HIT(caster, targetBlock);
        else if (spell.name == "Long Turtle Teleportation") EXECUTE_LONG_TURTLE_TELEPORTATION(caster, targetBlock, spell);
        else if (spell.name == "Aracashot") SUMMONS_AGGR_ARACHNEE_DISTANCE(caster, targetBlock, spell);
        else if (spell.name == "Call of Arachnee Major") EXECUTE_CALL_OF_ARACHNEE_MAJOR(caster, targetBlock, spell);
        else if (spell.name == "Call of Sick Arachnee") EXECUTE_CALL_OF_SICK_ARACHNEE(caster, targetBlock, spell);
        else if (spell.name == "Bomb Throw") EXECUTE_BOMB_THROW(caster, targetBlock, spell);
        else if (spell.name == "Sparoboom") EXECUTE_SPAROBOOM(caster, targetBlock, spell);
        else if (spell.name == "Swingewl") EXECUTE_SWINGEWL(caster, targetBlock);
        else if (spell.name == "White Rat Overhit") EXECUTE_RAT_ATTACK(caster, spell, 1);
        else if (spell.name == "Crocorage") EXECUTE_CROCORAGE(caster, spell);
        else if (spell.name == "Crocorage Chief") EXECUTE_CHIEF_CROCORAGE(caster, spell);
        else if (spell.name == "Survarmor") EXECUTE_SURVARMOR(caster, spell);
        else if (spell.name == "Nelween Power") EXECUTE_NELWEEN_POWER(caster, spell);
        else if (spell.name == "Talisman Power") EXECUTE_TALISMAN_POWER(caster, spell);
        else if (spell.name == "Arachnee Explosion") EXECUTE_ARACHNEE_EXPLOSION(caster, spell);
        else if (spell.name == "Flambe Tomb") EXECUTE_CALL_CHAFER_FLAMBE(caster, targetBlock, spell);
        else if (spell.name == "Black Rat Overhit") EXECUTE_RAT_ATTACK(caster, spell, 2);
        // ADD HERE ELSE IF (...) ...
        else Debug.LogError("Effect for " + spell.name + " has not implemented yet");
    }

    #endregion

    #region CHARACTER SPELLS SPECIALIZATION

    public static void EXECUTE_HELLBAK(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks()))
        {
            if (caster.summons.Count < caster.numberOfSummons)
                ut_execute_summon(caster, free, "Little Crobak", 2);
        }
    }

    public static void EXECUTE_SHOOTINBAK(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_repels(caster, targetBlock, 1);
        Block b = ut_repels(caster, targetBlock, 1);
        if (caster.summons.Count == caster.numberOfSummons) return;
        if (b != null)
            ut_execute_summon(caster, targetBlock, "Little Crobak", 2);
    }

    public static void EXECUTE_CHANGEBAK(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target.name.Contains("Little Crobak"))
            EXECUTE_TRANSPOSITION(caster, targetBlock);
    }

    public static void EXECUTE_FLOWING_DANCE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Coordinate oldCoord = caster.connectedCell.GetComponent<Block>().coordinate;
        EXECUTE_EXODUS(caster, targetBlock, s);
        Coordinate newCoord = caster.connectedCell.GetComponent<Block>().coordinate;
        if (oldCoord.equalsTo(newCoord)) return;
        if (oldCoord.row > newCoord.row)
            for (int i = newCoord.row; i <= oldCoord.row; i++) {
                Coordinate toCheck = new Coordinate(i, oldCoord.column);
                Block bToCheck = Map.Instance.getBlock(toCheck);
                if (bToCheck == null) continue;
                if (bToCheck.linkedObject == null) continue;
                Character target = bToCheck.linkedObject.GetComponent<Character>();
                if (target.team != caster.team)
                    target.inflictDamage(Spell.calculateDamage(caster, target, s));
            }
        else if (oldCoord.row < newCoord.row)
            for (int i = oldCoord.row; i <= newCoord.row; i++) {
                Coordinate toCheck = new Coordinate(i, oldCoord.column);
                Block bToCheck = Map.Instance.getBlock(toCheck);
                if (bToCheck == null) continue;
                if (bToCheck.linkedObject == null) continue;
                Character target = bToCheck.linkedObject.GetComponent<Character>();
                if (target.team != caster.team)
                    target.inflictDamage(Spell.calculateDamage(caster, target, s));
            }
        else if (oldCoord.column < newCoord.column)
            for (int i = oldCoord.column; i <= newCoord.column; i++) {
                Coordinate toCheck = new Coordinate(newCoord.row, i);
                Block bToCheck = Map.Instance.getBlock(toCheck);
                if (bToCheck == null) continue;
                if (bToCheck.linkedObject == null) continue;
                Character target = bToCheck.linkedObject.GetComponent<Character>();
                if (target.team != caster.team)
                    target.inflictDamage(Spell.calculateDamage(caster, target, s));
            }
        else if (oldCoord.column > newCoord.column)
            for (int i = newCoord.column; i <= oldCoord.column; i++) {
                Coordinate toCheck = new Coordinate(newCoord.row, i);
                Block bToCheck = Map.Instance.getBlock(toCheck);
                if (bToCheck == null) continue;
                if (bToCheck.linkedObject == null) continue;
                Character target = bToCheck.linkedObject.GetComponent<Character>();
                if (target.team != caster.team)
                    target.inflictDamage(Spell.calculateDamage(caster, target, s));
            }
    }

    public static void EXECUTE_BURNING_BONES(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_damageInLine(caster, targetBlock, s, 3, true);
        ut_comesCloser(caster, targetBlock, 2);
    }

    public static void EXECUTE_RAGING_SUN(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new RagingSunEvent("Raging Sun", target, s.effectDuration, ParentEvent.Mode.ActivationEachEndTurn, s.icon, caster, s));
    }

    public static void EXECUTE_WATER_WHEEL(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Character c in ut_getAdjacentHeroes(caster.connectedCell.GetComponent<Block>().coordinate)) {
            c.inflictDamage(calculateDamage(caster, c, s));
        }
    }
    
    public static void SUMMONS_PORTAL(Character caster, Block targetBlock, bool isFlexible) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!isFlexible) {
            Evocation p = ut_execute_summon(caster, targetBlock, "Portal", 3);
            if (p != null)
                p.isPortal = true;
        } else {
            Evocation p = ut_execute_summon(caster, targetBlock, "Flexible Portal", 3);
            if (p != null)
                p.isPortal = true;
        }
    }

    public static void EXECUTE_PORTAL_INTERRUPTION(Character caster) {
        if (!put_CheckArguments(new System.Object[] { caster })) return;
        int increment = ut_getAlliedPortals(caster);
        foreach (Character ally in ut_getAlliesWithCaster(caster)) {
            if (ally is Evocation) {
                if (((Evocation)ally).isPortal) {
                    ally.inflictDamage(ally.getActualHP() + ally.actual_shield);
                }
            }
            if (!ally.isDead) ally.incrementPA(increment);
        }
    }

    public static void EXECUTE_PORTAL_COALITION(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Block toSummonBlock = null;
        List<Block> frees = targetBlock.getFreeAdjacentBlocks();
        if (frees.Count == 0) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int indexResult = UnityEngine.Random.Range(0, frees.Count);
        toSummonBlock = frees[indexResult];
        SUMMONS_PORTAL(caster, toSummonBlock, false);
    }

    public static void EXECUTE_SINECURE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            if (((Evocation)target).isPortal) {
                List<Character> heroes = ut_getAllies(target);
                heroes.AddRange(ut_getEnemies(target));
                foreach (Character c in heroes) {
                    if (ut_isNearOf(target, c, 2)) {
                        c.inflictDamage(Spell.calculateDamage(caster, c, s) * 60 / 100);
                    }
                }
            }
        }
    }

    public static void EXECUTE_EXTRASENSORY_PERCEPTION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        int gain = 0;
        foreach (Character c in ut_getEnemies(caster)) {
            if (ut_isNearOf(caster, c, 8)) {
                gain++;
            }
        }
        if (gain == 0) return;
        if (gain > 6) gain = 6;
        else caster.incrementPM(gain);
    }

    public static void EXECUTE_INDOMITABLE_WILL(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new IndomitableWillEvent("Indomitable Will", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_OFFENCE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 25) {
            Block toSummonBlock = null;
            List<Block> frees = targetBlock.getFreeAdjacentBlocks();
            if (frees.Count == 0) return;
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int indexResult = UnityEngine.Random.Range(0, frees.Count);
            toSummonBlock = frees[indexResult];
            SUMMONS_PORTAL(caster, toSummonBlock, false);
        }
    }

    public static void EXECUTE_JUMP(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!caster.canMovedByEffects) return;
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

    public static void EXECUTE_TRUCE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        EXECUTE_TRANSPOSITION(caster, targetBlock);
        if (target != null) {
            TruceEvent te = new TruceEvent("Truce", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
            target.addEvent(te);
        }
    }

    public static void EXECUTE_ACCUMULATION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new AccumulationEvent("Accumulation", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }

    public static void EXECUTE_SMITHEREENS(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        DECREASE_RAGE_COUNTER(caster, 2);
        caster.addEvent(new SmithereensEvent("Smithereens", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
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

    public static void EXECUTE_UNCONSCIOUS_COMBAT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        if (caster.getActualPA() > 0) caster.decrementPA(caster.getActualPA());
        if (caster.getActualPM() > 0) caster.decrementPM(caster.getActualPM());
        UnconsciousCombatEvent e = new UnconsciousCombatEvent("Unconscious Combat", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(e);
        e.useIstantanely();
    }

    public static void EXECUTE_REFLEX(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        ReflexEvent e = new ReflexEvent("Reflex", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        caster.addEvent(e);
        e.useIstantanely();
    }

    public static void EXECUTE_FIGHT_BACK(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { s })) return;
        FightBackEvent fbe = new FightBackEvent("Fight Back", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(fbe);
    }

    public static void EXECUTE_COMPULSION(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        CompulsionEvent powerEvent = new CompulsionEvent("Compulsion", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(powerEvent);
        if (target.Equals(s.link))
            powerEvent.useIstantanely();
    }

    public static void EXECUTE_INFERNO(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        InfernoEvent ie = new InfernoEvent("Inferno", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(ie);
        ie.useIstantanely();
        Debug.Log("Inferno executed");
    }

    public static void EXECUTE_PAWERFUL(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.decrementRage(caster.rageCounter);
        PawerfulEvent paw = new PawerfulEvent("Pawerful", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(paw);
        paw.useIstantanely();
    }

    public static void EXECUTE_APPEASEMENT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (caster.rageCounter >= 2) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.receiveHeal(target.getTotalHP()*10/100 + caster.bonusHeal);
        }
        caster.decrementRage(caster.rageCounter);
    }

    public static void EXECUTE_WITHDRAWAL_ARROW(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_repelsCaster(caster, targetBlock, 2);
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

    public static void EXECUTE_IOP_WRATH(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new IopWrathEvent("Iop's Wrath", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }
    public static void EXECUTE_SWORD_OF_FATE(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new SwordOfFateEvent("Sword of Fate", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }
    public static void EXECUTE_PUNITIVE_ARROW(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.addEvent(new PunitiveArrowEvent("Punitive Arrow", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
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
            ch.receiveHeal((calculateDamage(caster, target, s) / 2) + caster.bonusHeal);
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

    public static void EXECUTE_REPULSIVE_ARROW(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Coordinate casterCoord = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetCoord = targetBlock.coordinate;
        if (casterCoord.column == targetCoord.column || casterCoord.row == targetCoord.row) {
            ut_repels(caster, targetBlock, 1);
        }
    }

    public static void EXECUTE_VERTEBRA(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Coordinate casterCoord = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetCoord = targetBlock.coordinate;
        if (casterCoord.column == targetCoord.column || casterCoord.row == targetCoord.row) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new VertebraEvent("Vertebra", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
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

    public static void EXECUTE_FULMINATING_ARROW(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        List<Character> allies = ut_getAllies(target);
        int distance = 10000;
        Character toDamage = null;
        if (allies.Count > 0)
            foreach(Character c in allies) {
                int act_dist = Monster.getDistance(target.connectedCell.GetComponent<Block>().coordinate, c.connectedCell.GetComponent<Block>().coordinate);
                if (act_dist < distance && act_dist != 0) {
                    distance = act_dist;
                    toDamage = c;
                }
            }
        if (toDamage != null && distance <= 8) {
            if (distance == 1) toDamage.inflictDamage(Spell.calculateDamage(caster, toDamage, s) * 90 / 100);
            if (distance == 2) toDamage.inflictDamage(Spell.calculateDamage(caster, toDamage, s) * 80 / 100);
            if (distance == 3) toDamage.inflictDamage(Spell.calculateDamage(caster, toDamage, s) * 75 / 100);
            if (distance == 4) toDamage.inflictDamage(Spell.calculateDamage(caster, toDamage, s) * 60 / 100);
            if (distance == 5) toDamage.inflictDamage(Spell.calculateDamage(caster, toDamage, s) * 50 / 100);
            if (distance == 6) toDamage.inflictDamage(Spell.calculateDamage(caster, toDamage, s) * 40 / 100);
            if (distance == 7) toDamage.inflictDamage(Spell.calculateDamage(caster, toDamage, s) * 20 / 100);
            if (distance == 8) toDamage.inflictDamage(Spell.calculateDamage(caster, toDamage, s) * 5 / 100);
        }
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
        if (s.name == "Exodus") {
            // Exodus specialization -> +1PM if the target is a portal
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            if (target is Evocation) {
                Evocation t = (Evocation)target;
                if (t.isPortal)
                    caster.incrementPM(1);
            }
        }
    }

    public static void EXECUTE_CONVULSION(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 2);
    }

    public static void EXECUTE_THERAPY(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 1);
    }

    public static void EXECUTE_CONTEMPT(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 2);
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
            SUMMONS_PORTAL(caster, actual, false);
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

    public static void EXECUTE_ATTRACTION(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 6);
    }

    public static void EXECUTE_THUNDERCLAP_FLASH(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Block casterBlock = caster.connectedCell.GetComponent<Block>();
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        Coordinate old = new Coordinate(casterBlock.coordinate.row, casterBlock.coordinate.column);
        EXECUTE_EXODUS(caster, targetBlock, s);
        casterBlock = caster.connectedCell.GetComponent<Block>();
        Coordinate newb = new Coordinate(casterBlock.coordinate.row, casterBlock.coordinate.column);
        List<Character> heroes = ut_getAllies(caster);
        heroes.AddRange(ut_getEnemies(caster));
        foreach (Character c in heroes) {
            if (ut_isNearOf(caster, c, 4) && !c.Equals(target)) {
                int damageToInflict = Spell.calculateDamage(caster, c, s);
                if (ut_isNearOf(caster, c, 1)) {
                    damageToInflict = damageToInflict * 70 / 100;
                } else if (ut_isNearOf(caster, c, 2)) {
                    damageToInflict = damageToInflict * 50 / 100;
                } else if (ut_isNearOf(caster, c, 3)) {
                    damageToInflict = damageToInflict * 30 / 100;
                } else if (ut_isNearOf(caster, c, 4)) {
                    damageToInflict = damageToInflict * 10 / 100;
                }
                c.inflictDamage(damageToInflict);
            }
        }
        if (!old.equalsTo(newb)) {
            ut_repelsCaster(caster, targetBlock, 3);
        }
    }
    public static void EXECUTE_GODLIKE_SPEED(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_comesCloser(caster, targetBlock, 20);
    }

    public static void EXECUTE_REPRISAL(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (targetBlock.linkedObject.GetComponent<Character>().isEnemyOf(caster)) return;
        ut_attracts(caster, targetBlock, 50);
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

    public static void EXECUTE_TRANSFUSION(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        Coordinate a = caster.connectedCell.GetComponent<Block>().coordinate;
        foreach (Character ch in ut_getAllies(caster))
        {
            Coordinate b = ch.connectedCell.GetComponent<Block>().coordinate;
            if (ut_isNearOf(a, b, 6))
            {
                ch.receiveHeal(100 + caster.bonusHeal);
            }
        }
    }
    public static void EXECUTE_SCUDO(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        Coordinate a = caster.connectedCell.GetComponent<Block>().coordinate;
        foreach (Character ch in ut_getAlliesWithCaster(caster))
        {
            Coordinate b = ch.connectedCell.GetComponent<Block>().coordinate;
            if (ut_isNearOf(a, b, 2))
            {
                ch.receiveShield(100 + caster.bonusGainShield);
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

    public static void EXECUTE_HEADS_OR_TAILS(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        targetBlock.linkedObject.GetComponent<Character>().addEvent(new HeadOrTailEvent("Heads or Tails", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster));
    }

    public static void EXECUTE_ALL_OR_NOTHING(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        targetBlock.linkedObject.GetComponent<Character>().addEvent(new AllOrNothingEvent("All or Nothing", targetBlock.linkedObject.GetComponent<Character>(), s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster));
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

    public static void EXECUTE_FELINE_SENSE(Character caster, Block targetBlock, Spell s) {
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
                    target.receiveHeal(50 + caster.bonusHeal);
                else Debug.Log("Feline sense failed");
            }
        }
    }

    public static void EXECUTE_EXPLOSIVE_ARROW(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        List<Character> heroes = ut_getAllies(target);
        heroes.AddRange(ut_getEnemies(target));
        foreach (Character c in heroes) {
            if (ut_isNearOf(c, target, 3)) {
                c.inflictDamage(Spell.calculateDamage(caster, c, s));
            }
        }
    }
    
    public static void EXECUTE_DEVASTATE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { s })) return;
        List<Character> heroes = ut_getAllies(caster);
        heroes.AddRange(ut_getEnemies(caster));
        foreach (Character c in heroes) {
            if (ut_isNearOf(caster, c, 2)) {
                c.inflictDamage(Spell.calculateDamage(caster, c, s));
            }
        }
    }

    public static void EXECUTE_SWORD_OF_YOP(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        List<Character> heroes = ut_getAlliesWithCaster(caster);
        heroes.AddRange(ut_getAllies(target));
        foreach (Character c in heroes) {
            if (ut_isNearOf(target, c, 2)) {
                c.inflictDamage(Spell.calculateDamage(caster, c, s));
            }
        }
    }

    public static void EXECUTE_SENTENCE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { s })) return;
        List<Character> heroes = ut_getAllies(caster);
        heroes.AddRange(ut_getEnemies(caster));
        foreach (Character c in heroes) {
            if (ut_isNearOf(caster, c, 5)) {
                int damageToInflict = Spell.calculateDamage(caster, c, s);
                if (ut_isNearOf(caster, c, 1)) {
                    damageToInflict = damageToInflict * 100 / 100;
                } else if (ut_isNearOf(caster, c, 2)) {
                    damageToInflict = damageToInflict * 80 / 100;
                } else if(ut_isNearOf(caster, c, 3)) {
                    damageToInflict = damageToInflict * 65 / 100;
                } else if (ut_isNearOf(caster, c, 4)) {
                    damageToInflict = damageToInflict * 50 / 100;
                } else if(ut_isNearOf(caster, c, 5)) {
                    damageToInflict = damageToInflict * 30 / 100;
                }
                c.inflictDamage(damageToInflict);
            }
        }
    }

    public static void EXECUTE_CELESTIAL_SWORD(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        Coordinate a = targetBlock.coordinate;
        List<Character> heroes = ut_getAllies(target);
        heroes.AddRange(ut_getEnemies(target));
        foreach (Character c in heroes) {
            Coordinate b = c.connectedCell.GetComponent<Block>().coordinate;
            if (ut_isNearOf(a, b, 2)) {
                c.inflictDamage(Spell.calculateDamage(caster, c, s));
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
            if (t.Item1.isDead || t.Item1 is Evocation || t.Item1 is MonsterEvocation) continue;
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

    public static void EXECUTE_KARCHAM(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Coordinate c = caster.connectedCell.GetComponent<Block>().coordinate;
        List<Character> adj_heroes = ut_getAdjacentHeroes(c);
        if (adj_heroes == null || adj_heroes.Count == 0) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        Character chosen = adj_heroes[UnityEngine.Random.Range(0, adj_heroes.Count)];
        EXECUTE_JUMP(chosen, targetBlock);
    }

    public static void EXECUTE_CAPERING(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        EXECUTE_JUMP(caster, targetBlock);
        Coordinate c = targetBlock.coordinate;
        List<Character> adj_heroes = ut_getAdjacentHeroes(c);
        foreach (Character adj in adj_heroes)
            adj.inflictDamage(calculateDamage(caster, adj, s));
    }

    public static void EXECUTE_STUNT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        EXECUTE_JUMP(caster, targetBlock);
        Coordinate c = targetBlock.coordinate;
        List<Character> adj_heroes = ut_getAdjacentHeroes(c);
        foreach (Character adj in adj_heroes)
            adj.inflictDamage(calculateDamage(caster, adj, s));
        StuntEvent powerEvent = new StuntEvent("Stunt", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(powerEvent);
        powerEvent.useIstantanely();
    }

    public static void EXECUTE_VIOLENCE(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Coordinate c = targetBlock.coordinate;
        List<Character> adj_heroes = ut_getAdjacentHeroes(c);
        bool enemyFound = false;
        foreach (Character adj in adj_heroes)
            if (adj.isEnemyOf(caster)) enemyFound = true;
        if (enemyFound)
        {
            ViolenceEvent ve = new ViolenceEvent("Violence", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            caster.addEvent(ve);
            ve.useIstantanely();
        }
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

    public static void EXECUTE_PREY(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        PreyEvent he = new PreyEvent("Prey", target, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        target.addEvent(he);
        he.useIstantanely();
    }
    public static void EXECUTE_FURIOUS_HUNTING(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        FuriousHuntingEvent he = new FuriousHuntingEvent("Furious Hunting", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, target);
        caster.addEvent(he);
    }
    public static void EXECUTE_BLOODHOUND(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Coordinate tbc = targetBlock.coordinate;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target.getEventSystem().getEvents("Prey").Count > 0)
            foreach (Character c in ut_getAllies(caster)) {
                Coordinate cc = c.connectedCell.GetComponent<Block>().coordinate;
                if (cc.row == tbc.row || cc.column == tbc.column) {
                    ut_attracts(target, c.connectedCell.GetComponent<Block>(), 2);
                }
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

    public static void EXECUTE_TORTUGA(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character c = targetBlock.linkedObject.GetComponent<Character>();
        TortugaEvent se = new TortugaEvent("Tortuga", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, caster);
        c.addEvent(se);
        se.useIstantanely();
    }

    public static void EXECUTE_FURIA(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_comesCloser(caster, targetBlock, 2);
    }

    public static void EXECUTE_TAILING(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_comesCloser(caster, targetBlock, 3);
    }

    public static void EXECUTE_COMEDY(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_comesCloser(caster, targetBlock, 2);
        ut_repels(caster, targetBlock, 4);
    }
    public static void EXECUTE_BOLICHE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 5);
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

    public static void EXECUTE_PURSUIT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAlliesWithCaster(caster)) {
            if (ut_isNearOf(caster, c, 3)) {
                if (caster.Equals(c)) {
                    PursuitEvent le = new PursuitEvent("Pursuit", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
                    caster.addEvent(le);
                    le.useIstantanely();
                } else c.addEvent(new PursuitEvent("Pursuit", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
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

    public static void EXECUTE_AEGIS_ARMOR(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster))
            if (ut_isNearOf(c, caster, 3)) {
                AegisArmorEvent pg = new AegisArmorEvent("Aegis Armor", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
                c.addEvent(pg);
                pg.useIstantanely();
            }
    }

    public static void EXECUTE_FRIENDLY_ARMOR(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        FriendlyArmorEvent pg = new FriendlyArmorEvent("Friendly Armor", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        target.addEvent(pg);
        pg.useIstantanely();
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

    public static void EXECUTE_FORTIFICATION(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new FortificationEvent("Fortification", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster));
    }


    public static void EXECUTE_HELPING_WORD(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new HelpingWordEvent("Helping Word", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster));
    }

    public static void EXECUTE_BONTAO(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster)) {
            c.receiveHeal(s.damage + caster.bonusHeal);
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
                if (c is Evocation) if (((Evocation)c).isRune) continue;
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
    public static void EXECUTE_HUPPERDIE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.inflictDamage(Spell.calculateDamage(caster, target, s));
        if (target.isDead)
            SUMMONS_RUNE(caster, targetBlock);
    }
    public static void EXECUTE_FRIGHTENING_WORD(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_repels(caster, targetBlock, 1);
    }

    public static void EXECUTE_OVERCHARGE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        int remaining_pa = caster.getActualPA();
        if (remaining_pa > 0)
            caster.decrementPA(remaining_pa);
        int bonus_shield = remaining_pa * 40;
        caster.receiveShield(s.damage + bonus_shield + caster.bonusGainShield);
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
                c.receiveHeal(30 + caster.bonusHeal);
    }
    public static void EXECUTE_TRANCE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        int toDamage = caster.getTotalHP() * 80 / 100;
        caster.inflictDamage(toDamage, false, true);
        if (caster.isDead) return;
        List<Character> heroes = ut_getAdjacentHeroes(targetBlock.coordinate);
        heroes.Add(caster);
        foreach (Character c in heroes)
            if (!c.isEnemyOf(caster))
                c.receiveShield(toDamage / 2 + caster.bonusGainShield);
    }

    public static void EXECUTE_STRIKING_WORD(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Character ally in ut_getAllies(caster))
            if (ut_isNearOf(ally, targetBlock.linkedObject.GetComponent<Character>(), 3))
                ally.addEvent(new StrikingWordEvent("Striking Word", ally, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster));
    }

    public static void EXECUTE_TEAM_WORD(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Character ally in ut_getAllies(caster))
            if (ut_isNearOf(ally, caster, 2))
                ally.receiveHeal(20 + caster.bonusHeal);
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
        PreventingWordEvent pw = new PreventingWordEvent("Preventing Word", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, caster);
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

    public static void EXECUTE_BLOODY_HOSTILITY(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        int damage = calculateDamage(caster, caster, s);
        caster.inflictDamage(damage * 30 / 100, false, true);
    }
    public static void EXECUTE_BLOODY_PUNISHMENT(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        caster.inflictDamage(calculateDamage(caster, caster, s), false, true);
    }

    public static void EXECUTE_PARALYSING_WORD(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new ParalysingWordEvent("Paralysing Word", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_CAT_ASSAULT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (caster.summons.Count > 0) {
            Evocation cat = caster.summons[0];
            List<Block> frees = targetBlock.getFreeAdjacentBlocks();
            if (frees.Count == 0) return;
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int indexResult = UnityEngine.Random.Range(0, frees.Count);
            Block toTeleport = frees[indexResult];
            EXECUTE_JUMP(cat, toTeleport);
        }
    }

    public static void EXECUTE_CAT_SOUL(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (caster.summons.Count > 0) {
            Evocation cat = caster.summons[0];
            if (Monster.getDistance(cat.connectedCell.GetComponent<Block>().coordinate, targetBlock.coordinate) == 1)
                cat.receiveHeal(22 + caster.bonusHeal);
        }
    }

    public static void SUMMONS_BWORK_MAGE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Bwork_Mage", 3);
    }
    public static void SUMMONS_CUNEY(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Protective Coney", 2);
    }
    public static void SUMMONS_PINT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation p = ut_execute_summon(caster, targetBlock, "Pandawa Pint", 2);
        p.isPint = true;
    }

    public static void SUMMONS_AEGIS(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Aegis", 1);
    }

    public static void SUMMONS_CLAW(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Chasquatch", 2);
    }

    public static void SUMMONS_CRAQUELEUR(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Craqueleur", 1);
    }
    public static void SUMMONS_FLYING_SWORD(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Flying Sword", 2);
    }

    public static void SUMMONS_GOBBALL(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Gobball", 1);
    }

    public static void SUMMONS_ELEMENTAL_GUARDIAN(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation e = ut_execute_summon(caster, targetBlock, "Elemental Guardian", 1);
        if (e != null)
            e.isGuardian = true;
    }
    public static void SUMMONS_RUNE(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        Evocation r = null;
        int prob = UnityEngine.Random.Range(0, 4);
        if (prob == 0)
        {
            r = ut_execute_summon(caster, targetBlock, "Fire Rune", 2);
        }
        else if (prob == 1)
        {
            r = ut_execute_summon(caster, targetBlock, "Earth Rune", 2);
        }
        else if (prob == 2)
        {
            r = ut_execute_summon(caster, targetBlock, "Air Rune", 2);
        }
        else if (prob == 3)
        {
            r = ut_execute_summon(caster, targetBlock, "Water Rune", 2);
        }
        if (r != null)
            r.isRune = true;
    }

    public static void EXECUTE_MASS_GRAVE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        List<Character> enemies = Spell.ut_getEnemies(caster);
        List<Block> pickedBlocks = new List<Block>();
        foreach (Character enemy in enemies)
        {
            List<Block> freeBlocks = enemy.connectedCell.GetComponent<Block>().getFreeAdjacentBlocks();
            foreach (Block toRem in pickedBlocks)
            {
                for (int i = 0; i < freeBlocks.Count; i++)
                {
                    if (toRem.equalsTo(freeBlocks[i]))
                    {
                        freeBlocks.RemoveAt(i);
                        i--;
                    }
                }
            }
            if (freeBlocks.Count == 0) continue;
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int indexResult = UnityEngine.Random.Range(0, freeBlocks.Count);
            if (ut_getDeadStatsAllies(caster).Item1 > 1)
                Spell.ut_execute_summon(caster, freeBlocks[indexResult], "Soul Chafer", 3);
            else
                Spell.ut_execute_summon(caster, freeBlocks[indexResult], "Chafer", 2);
            pickedBlocks.Add(freeBlocks[indexResult]);
        }
    }

    public static void EXECUTE_DEATH_CON(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        DeathConEvent cde_caster, cde_target;
        cde_caster = new DeathConEvent("Death Con", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, true);
        cde_target = new DeathConEvent("Death Con", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, false);
        caster.addEvent(cde_caster);
        target.addEvent(cde_target);
        cde_caster.useIstantanely();
        cde_target.useIstantanely();
    }
    public static void EXECUTE_PERFIDY(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        int deads = ut_getDeadStatsAllies(caster).Item2;
        if (deads > 3) caster.incrementPA(3);
        else caster.incrementPA(deads);
    }

    public static void SUMMONS_TOFU(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Tofu", 3);
    }

    public static void XELOR_DIAL(Character caster, Block targetBlock)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation dial = ut_execute_summon(caster, targetBlock, "Xelor Dial", 2);
        if (dial != null)
            dial.setDial();
    }

    public static void EXECUTE_TEMPORAL_PARADOX(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        EXECUTE_TRANSPOSITION(caster, targetBlock);
        if (target is Evocation)
        {
            if (((Evocation)target).isDial)
            {
                caster.addEvent(new TemporalParadox("Temporal Paradox", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
            }
        }
    }
    public static void EXECUTE_OVERCLOCK(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        List<Character> adjacentChars = ut_getAdjacentHeroes(caster.connectedCell.GetComponent<Block>().coordinate);
        bool isNear = false;
        foreach(Character c in adjacentChars)
        {
            if (c is Evocation)
            {
                if (((Evocation)c).isDial)
                {
                    isNear = true;
                    break;
                }
            }
        }
        if (isNear) caster.incrementPA(2);
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 40)
        {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new OverclockEvent("Overclock", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void EXECUTE_PETRIFICATION(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 80)
        {
            int paToLose = 2;
            List<Character> adjacentChars = ut_getAdjacentHeroes(targetBlock.coordinate);
            bool isNear = false;
            foreach (Character c in adjacentChars)
            {
                if (c is Evocation)
                {
                    if (((Evocation)c).isDial)
                    {
                        isNear = true;
                        break;
                    }
                }
            }
            if (isNear) paToLose = 3;
            Debug.Log("PETRIFICATION ACTING");
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new PetrificationEvent("Petrification", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, paToLose));
        }
    }

    public static void SUMMONS_PRESPIC(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Prespic", 3);
    }

    public static void SUMMONS_SHELLY(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation sh = ut_execute_summon(caster, targetBlock, "Shelly", 3);
        sh.isShelly = true;
    }

    public static void SUMMONS_SCAPHANDER(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            if (((Evocation)target).isShelly) {
                target.inflictDamage(target.getActualHP() + target.actual_shield);
                Evocation sh = ut_execute_summon(caster, targetBlock, "Scaphander", 1);
            }
        }
    }

    public static void SUMMONS_DRAGONNET(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Dragonnet", 2);
    }
    
    public static void EXECUTE_NATURAL_ATTRACTION(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 2);
    }

    public static void SUMMONS_PANDAWASTA(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Pandawasta", 2);
    }

    public static void SUMMONS_BAMBOO(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Bamboo", 3);
    }

    public static void SUMMONS_MADOLL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Madoll", 3);
    }

    public static void SUMMONS_INFLATABLE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Inflatable", 2);
    }

    public static void SUMMONS_SACRIFICIAL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Sacrificial_Doll", 2);
    }

    public static void SUMMONS_THE_BLOCK(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "The_Block", 1);
    }

    public static void SUMMONS_TREE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Tree", 1);
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

    public static void EXECUTE_SONAR(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach(Evocation e in caster.summons) {
            if (e.isShelly) e.addEvent(new SonarEvent("Sonar", e, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
        }
    }

    public static void EXECUTE_RUNIC_OVERCHARGE(Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation)
            if (((Evocation)target).isRune)
                ((Evocation)target).incrementRunicPower();
    }

    public static void EXECUTE_SUN_LANCE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation)
            if (((Evocation)target).isRune) {
                ((Evocation)target).runeExplosion();
                return;
            }
        target.inflictDamage(Spell.calculateDamage(caster, target, s));
    }

    public static void EXECUTE_ELEMENTAL_DRAIN(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        bool found = false;
        foreach(Character c in Spell.ut_getAllies(caster))
        {
            if (c is Evocation) {
                if (((Evocation)c).isRune)
                {
                    found = true;
                    c.inflictDamage(c.getActualHP() * 30 / 100);
                }
            }
        }

        if (found)
            foreach (Character c in Spell.ut_getAllies(caster))
            {
                if (c is Evocation)
                {
                    if (((Evocation)c).isGuardian)
                    {
                        c.addEvent(new ElementalDrainEvent("Elemental Drain", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
                    }
                }
            }
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

    public static void EXECUTE_JAW(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (caster.rageCounter >= 2)
            EXECUTE_EXODUS(caster, targetBlock, s);
    }

    public static void EXECUTE_WATERFALL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        // Manual check to avoid ID check
        if (target is Evocation && target.name == "Bamboo" && target.team == caster.team) {
            foreach(Character c in ut_getAllies(caster)) {
                if (c.name != "Bamboo")
                    c.receiveHeal(20 + caster.bonusHeal);
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

    public static void EXECUTE_TUMULT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new TumultEvent("Tumult", target, s.effectDuration, ParentEvent.Mode.ActivationEachEndTurn, s.icon, caster, s));
    }

    public static void EXECUTE_EARTHQUAKE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(caster, enemy, 6)) {
                enemy.addEvent(new EarthquakeEvent("Earthquake", enemy, s.effectDuration, ParentEvent.Mode.ActivationEachEndTurn, s.icon, caster, s));
            }
    }

    public static void EXECUTE_MUSKET(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (caster.summons.Count == caster.numberOfSummons) return;
        Block toSummonBlock = null;
        List<Block> frees = targetBlock.getFreeAdjacentBlocks();
        if (frees.Count == 0) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int indexResult = UnityEngine.Random.Range(0, frees.Count);
        toSummonBlock = frees[indexResult];
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(0, 3);
        if (prob == 0)
        {   // Explobombe
            Evocation bomb = ut_execute_summon(caster, toSummonBlock, "Explobombe", 2);
            bomb.setBomb(caster, caster.spells[4]);
        } else if (prob == 1)
        {   // Tornabombe
            Evocation bomb = ut_execute_summon(caster, toSummonBlock, "Tornabombe", 2);
            bomb.setBomb(caster, caster.spells[5]);
        } else if (prob == 2)
        {   // Waterbombe
            Evocation bomb = ut_execute_summon(caster, toSummonBlock, "Waterbombe", 2);
            bomb.setBomb(caster, caster.spells[6]);
        }
    }

    public static void EXECUTE_BOMBARD(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_repels(caster, targetBlock, 1);
        if (caster.summons.Count == caster.numberOfSummons) return;

        List<Block> frees = caster.connectedCell.GetComponent<Block>().getFreeAdjacentBlocks();
        if (frees.Count == 0) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int indexResult = UnityEngine.Random.Range(0, frees.Count);
        Block toSummonBlock = frees[indexResult];
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(0, 3);
        if (prob == 0)
        {   // Explobombe
            Evocation bomb = ut_execute_summon(caster, toSummonBlock, "Explobombe", 2);
            bomb.setBomb(caster, caster.spells[4]);
        }
        else if (prob == 1)
        {   // Tornabombe
            Evocation bomb = ut_execute_summon(caster, toSummonBlock, "Tornabombe", 2);
            bomb.setBomb(caster, caster.spells[5]);
        }
        else if (prob == 2)
        {   // Waterbombe
            Evocation bomb = ut_execute_summon(caster, toSummonBlock, "Waterbombe", 2);
            bomb.setBomb(caster, caster.spells[6]);
        }
    }

    public static void EXECUTE_CADENCE(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        int npm = caster.summons.Count;
        if (npm > 0)
            target.addEvent(new CadenceEvent("Cadence", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, npm));
    }

    public static void EXECUTE_BOMB_STRATEGY(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        int npa = caster.summons.Count;
        if (npa > 0)
            caster.incrementPA(npa);
        EXECUTE_DETONATOR(caster, caster.connectedCell.GetComponent<Block>());
    }

    public static void EXECUTE_DOLL_SACRIFICE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character enemy in ut_getEnemies(caster))
            if (!enemy.EqualsNames(target) && ut_isNearOf(caster, enemy, 2)) {
                enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
            }
        foreach (Character enemy in ut_getAllies(caster))
            if (!enemy.EqualsNames(target) && ut_isNearOf(caster, enemy, 2)) {
                enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
            }
        caster.inflictDamage(caster.actual_hp + caster.actual_shield);
    }
    public static void EXECUTE_DOLL_SCREAM(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new DollScreamEvent("Doll Scream", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void SUMMONS_EXPLOBOMBE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Evocation bomb = ut_execute_summon(caster, targetBlock, "Explobombe", 2);
        bomb.setBomb(caster, s);
    }

    public static void SUMMONS_TORNABOMBE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Evocation bomb = ut_execute_summon(caster, targetBlock, "Tornabombe", 2);
        bomb.setBomb(caster, s);
    }

    public static void SUMMONS_WATERBOMBE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        Evocation bomb = ut_execute_summon(caster, targetBlock, "Waterbombe", 2);
        bomb.setBomb(caster, s);
    }

    public static void SUMMONS_LIVING_SHOVEL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Living Shovel", 2);
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

    public static void SUBEXECUTE_EXPLOSION(MonsterEvocation bomb) {
        if (!put_CheckArguments(new System.Object[] { bomb })) return;
        List<Character> allHeroes = ut_getEnemies(bomb);
        allHeroes.AddRange(ut_getAllies(bomb));
        foreach (Character ch in allHeroes) {
            if (ut_isNearOf(ch, bomb, 2)) {
                if (ch is MonsterEvocation) {
                    if (((MonsterEvocation)ch).isBomb && !bomb.connectedSummoner.monsterSummons.Contains((MonsterEvocation)ch)) {
                        ch.inflictDamage(bomb.getBombDamage(ch));
                    } else if (!((MonsterEvocation)ch).isBomb) {
                        ch.inflictDamage(bomb.getBombDamage(ch));
                    }
                    // don't execute damage on summoner bombs
                } else {
                    ch.inflictDamage(bomb.getBombDamage(ch));
                }
            }
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
                evoTarget.inflictDamage(evoTarget.actual_hp + evoTarget.actual_shield);
            }
        } else if (target.Equals(caster)) {
            // get all bombs from the caster and execute explosion
            List<Evocation> temp_summons = new List<Evocation>();
            foreach (Evocation evoTarget in caster.summons) {
                SUBEXECUTE_EXPLOSION(evoTarget, false);
                temp_summons.Add(evoTarget);
            }
            foreach (Evocation evoTarget in temp_summons) {
                evoTarget.inflictDamage(evoTarget.actual_hp + evoTarget.actual_shield);
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

    public static void EXECUTE_DESTRUCTIVE_ARROW(Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 40)
        {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new DestructiveArrowEvent("Destructive Arrow", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
        }
    }

    public static void EXECUTE_IMMOBILISING_ARROW(Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int prob = UnityEngine.Random.Range(1, 101);
        Debug.Log("Spell " + s.name + " prob: " + prob);
        if (prob <= 80)
        {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            target.addEvent(new ImmobilisingArrowEvent("Immobilising Arrow", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
    }

    public static void SUMMONS_SENTINEL_TURRECT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation turrect = ut_execute_summon(caster, targetBlock, "Sentinel_Turret", 1);
        if (turrect != null)
            turrect.isTurrect = true;
    }

    public static void SUMMONS_GUARDIANA_TURRECT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation turrect = ut_execute_summon(caster, targetBlock, "Guardiana_Turret", 2);
        if (turrect != null)
            turrect.isTurrect = true;
    }

    public static void SUMMONS_TACTICAL_TURRECT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        Evocation turrect = ut_execute_summon(caster, targetBlock, "Tactical_Turret", 3);
        if (turrect != null)
            turrect.isTurrect = true;
    }

    public static void SUMMONS_TACTICAL_BEACON(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_execute_summon(caster, targetBlock, "Tactical Beacon", 3);
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
        Evocation dd = ut_execute_summon(caster, targetBlock, "Double_" + caster.name, 1);
        if (dd != null) dd.isDouble = true;
    }

    public static void EXECUTE_MIST(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(enemy, caster, 3)) {
                MistEvent me = new MistEvent("Mist", enemy, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
                enemy.addEvent(me);
                me.useIstantanely();
            }
    }

    public static void SUMMONS_CHAFERFU(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (ut_getDeadStatsAllies(caster).Item1 > 1)
            ut_execute_summon(caster, targetBlock, "Chafer", 2);
        else
            ut_execute_summon(caster, targetBlock, "Chafer_Lancer", 1);
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
                c.receiveHeal(20 + caster.bonusHeal);
    }

    public static void EXECUTE_BUBBLE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character c in ut_getAllies(caster))
            if (ut_isNearOf(c, target, 2))
                c.receiveShield(Spell.calculateDamage(caster, c, s) / 2);
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

    public static void ADD_RAGE_COUNTER(Character caster, int value) {
        if (!put_CheckArguments(new System.Object[] { caster })) return;
        caster.incrementRage(value);
        if (caster.rageCounter == 5 && caster.getEventSystem().getEvents("Uginak Rage").Count == 0) {
            UginakRage ur = new UginakRage("Uginak Rage", caster, 3, ParentEvent.Mode.PermanentAndEachTurn, Resources.Load<Sprite>("Prefabs/Heroes/Transformation/Uginak Rage"));
            caster.addEvent(ur);
            ur.useIstantanely();
            caster.decrementRage(5);
            Block toSummonBlock = null;
            List<Block> frees = caster.connectedCell.GetComponent<Block>().getFreeAdjacentBlocks();
            if (frees.Count == 0) return;
            UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
            int indexResult = UnityEngine.Random.Range(0, frees.Count);
            toSummonBlock = frees[indexResult];
            Evocation p = ut_execute_summon(caster, toSummonBlock, "Yapper", 2);
            if (p != null)
                p.isYapper = true;
        }
    }

    public static void DECREASE_RAGE_COUNTER(Character caster, int value) {
        if (!put_CheckArguments(new System.Object[] { caster })) return;
        caster.decrementRage(value);
    }

    public static void EXECUTE_CERBERUS(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (caster.rageCounter >= 2) {
            Character c = targetBlock.linkedObject.GetComponent<Character>();
            c.addEvent(new CerberusEvent("Cerberus", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
        caster.decrementRage(2);
    }

    public static void EXECUTE_BARK(Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character c = targetBlock.linkedObject.GetComponent<Character>();
        BarkEvent be = new BarkEvent("Bark", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        c.addEvent(be);
        be.useIstantanely();
    }

    public static void EXECUTE_FRACTURE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (caster.getEventSystem().getEvents("Yop God Status").Count > 0) {
            FractureEvent fe = new FractureEvent("Fracture", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            target.addEvent(fe);
            fe.useIstantanely();
        }
        foreach (Character enemy in ut_getAllies(target))
            if (ut_isNearOf(target, enemy, 2)) {
                enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
                if (caster.getEventSystem().getEvents("Yop God Status").Count > 0) {
                    FractureEvent fenew = new FractureEvent("Fracture", enemy, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
                    enemy.addEvent(fenew);
                    fenew.useIstantanely();
                }
            }
    }

    public static void EXECUTE_DESTRUCTIVE_RING(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        List<Character> adjs = ut_getAdjacentHeroes(targetBlock.coordinate);
        foreach (Character c in adjs) {
            c.inflictDamage(Spell.calculateDamage(caster, c, s));
            if (caster.getEventSystem().getEvents("Yop God Status").Count > 0)
                ut_repels(target, c.connectedCell.GetComponent<Block>(), 1);
        }
    }

    public static void EXECUTE_WAKMEHA(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_damageInLine(caster, targetBlock, s, 5);
        if (caster.getEventSystem().getEvents("Wakfu Raider Status").Count > 0)
        {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            if (target is Evocation)
            {
                Evocation e = (Evocation)target;
                if (e.isWakfuTotem || e.isPortal)
                {
                    EXECUTE_TRANSPOSITION(caster, targetBlock);
                }
            }
        }
    }
    public static void EXECUTE_REDEMPTION_ARROW(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_damageInLine(caster, targetBlock, s, 6, true);
    }

    public static void EXECUTE_AFFRONT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();

        foreach (Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(target, enemy, 2) && !target.Equals(enemy))
                enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));

        if (caster.getEventSystem().getEvents("Wakfu Raider Status").Count > 0) {
            if (target is Evocation) {
                Evocation e = (Evocation)target;
                if (e.isWakfuTotem || e.isPortal) {
                    EXECUTE_TRANSPOSITION(caster, targetBlock);
                }
            }
        }
    }

    public static void EXECUTE_NEUTRAL(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation || target is MonsterEvocation) {
            if (target is Evocation)
                if (((Evocation)target).isPortal)
                    caster.incrementPM(2);
            target.inflictDamage(target.actual_hp + target.actual_shield);
        }
    }

    public static void EXECUTE_AUDACIOUS(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (caster.getEventSystem().getEvents("Wakfu Raider Status").Count > 0) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            if (target is Evocation) {
                Evocation e = (Evocation)target;
                if (e.isWakfuTotem || e.isPortal) {
                    ut_repels(caster, targetBlock, 3);
                }
            }
        }
    }

    public static void EXECUTE_LIGHTNING_FIST(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (caster.getEventSystem().getEvents("Wakfu Raider Status").Count > 0) {
            Character target = targetBlock.linkedObject.GetComponent<Character>();
            if (target is Evocation) {
                Evocation e = (Evocation)target;
                if (e.isWakfuTotem || e.isPortal) {
                    foreach (Character enemy in ut_getEnemies(caster)) {
                        List<Block> blocks = enemy.connectedCell.GetComponent<Block>().getFreeAdjacentBlocksWithCharacters(caster.team);
                        foreach (Block b in blocks) {
                            if (b.linkedObject != null) {
                                Character c_inBlock = b.linkedObject.GetComponent<Character>();
                                if (c_inBlock is Evocation) {
                                    Evocation e_inBlock = (Evocation)c_inBlock;
                                    if (e_inBlock.isWakfuTotem || e.isPortal) {
                                        enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
                                        break;
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
    }

    #endregion

    #region MONSTER SPELLS SPECIALIZATION

    public static void EXECUTE_BRIKOCOOP(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster))
            c.receiveHeal(s.damage);
    }

    public static void EXECUTE_RAT_ATTACK(Character caster, Spell s, int type) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        if (type == 1) { // white
            WhiteRatOverhitEvent wro = new WhiteRatOverhitEvent("White Rat Overhit", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            caster.addEvent(wro);
            wro.useIstantanely();
        }
        if (type == 2) { // black
            BlackRatOverhitEvent wro = new BlackRatOverhitEvent("Black Rat Overhit", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            caster.addEvent(wro);
            wro.useIstantanely();
        }
    }

    public static void EXECUTE_KANNILANCE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        ut_damageInLine(caster, targetBlock, s, 3);
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

    public static void EXECUTE_EXPLOSIVE_EGG(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character c in ut_getAdjacentHeroes(targetBlock.coordinate)) {
            if (!c.isEnemyOf(target)) {
                c.inflictDamage(Spell.calculateDamage(caster, c, s));
            }
        }
    }

    public static void EXECUTE_GROUP_WABBHEAL(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character ch in ut_getAllies(caster)) {
            if (ut_isNearOf(ch, caster, 2))
                ch.receiveHeal(s.damage);
        }
    }

    public static void EXECUTE_GWANDHIT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character c in ut_getAllies(target)) {
            c.inflictDamage(Spell.calculateDamage(caster, c, s) * 20 / 100);
        }
    }

    public static void EXECUTE_MY_TOFU_CHILDS(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            ut_execute_monsterSummon(caster, free, "Tofu Doll");
        }
    }
    public static void EXECUTE_CALL_OF_ARACHNEE_MAJOR(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            ut_execute_monsterSummon(caster, free, "Alpha Arachnee");
        }
    }
    public static void EXECUTE_ATTRACTIVE_SMELL(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            ut_execute_monsterSummon(caster, free, "Moskito");
        }
    }

    public static void EXECUTE_ARACHNEE_POPULATION(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            ut_execute_monsterSummon(caster, free, "Arachnausea Doll");
        }
    }

    public static void EXECUTE_CALL_OF_THE_FOREST(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Monster casterMonster = (Monster)caster;
        Character closest = casterMonster.getClosestEnemy();
        if (closest == null) return;
        Block closestBlock = closest.connectedCell.GetComponent<Block>();
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, closestBlock.coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        ut_execute_monsterSummon(caster, toSummonBlock, "Bear");
    }

    public static void EXECUTE_CALL_CHAFER_FLAMBE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Monster casterMonster = (Monster)caster;
        Character closest = casterMonster.getClosestEnemy();
        if (closest == null) return;
        Block closestBlock = closest.connectedCell.GetComponent<Block>();
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (caster.connectedCell.GetComponent<Block>().getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, closestBlock.coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        ut_execute_monsterSummon(caster, toSummonBlock, "Chafer Flambe");
    }

    public static void EXECUTE_TALISMAN_POWER(Character caster, Spell s) {
        foreach(Character c in ut_getEnemies(caster)) {
            c.addEvent(new TalismanPowerEvent("Talisman Power", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        }
        MonsterTalismanPowerEvent mtpe = new MonsterTalismanPowerEvent("Talisman Power", caster, s.effectDuration+1, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(mtpe);
        mtpe.useIstantanely();
    }

    public static void EXECUTE_CALL_OF_SICK_ARACHNEE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Monster casterMonster = (Monster)caster;
        Character closest = casterMonster.getClosestEnemy();
        if (closest == null) return;
        Block closestBlock = closest.connectedCell.GetComponent<Block>();
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, closestBlock.coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        MonsterEvocation e = ut_execute_monsterSummon(caster, toSummonBlock, "Sick Arachnee");
        if (casterMonster.getCompleteName().Contains("Branch")) {
            e.hp = e.getTotalHP() / 2;
            e.actual_hp = e.hp;
        }
    }

    public static void EXECUTE_CALL_OF_THE_CAT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Monster casterMonster = (Monster)caster;
        Character closest = casterMonster.getClosestEnemy();
        if (closest == null) return;
        Block closestBlock = closest.connectedCell.GetComponent<Block>();
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, closestBlock.coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        ut_execute_monsterSummon(caster, toSummonBlock, "Dagobert");
    }

    public static void EXECUTE_CALL_OF_THE_MEKA(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Monster casterMonster = (Monster)caster;
        Character closest = casterMonster.getClosestEnemy();
        if (closest == null) return;
        Block closestBlock = closest.connectedCell.GetComponent<Block>();
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, closestBlock.coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int i = UnityEngine.Random.Range(0, 3);
        if (i == 0)
            ut_execute_monsterSummon(caster, toSummonBlock, "Pushy Robot");
        else if (i == 1)
            ut_execute_monsterSummon(caster, toSummonBlock, "Robo Mace");
        else if (i == 2)
            ut_execute_monsterSummon(caster, toSummonBlock, "Robionicle");
    }

    public static void EXECUTE_CALL_OF_ARACHNEE(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Monster casterMonster = (Monster)caster;
        Character closest = casterMonster.getClosestEnemy();
        if (closest == null) return;
        Block closestBlock = closest.connectedCell.GetComponent<Block>();
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks()))
        {
            int actualDistance = Monster.getDistance(free.coordinate, closestBlock.coordinate);
            if (actualDistance < bestDistance)
            {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        ut_execute_monsterSummon(caster, toSummonBlock, "Alpha Arachnee");
    }

    public static void EXECUTE_CALL_OF_BIBLOP(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Monster casterMonster = (Monster)caster;
        Character closest = casterMonster.getClosestEnemy();
        if (closest == null) return;
        Block closestBlock = closest.connectedCell.GetComponent<Block>();
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, closestBlock.coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        Element e = caster.spells[0].element;
        if (e == Element.Air)
            ut_execute_monsterSummon(caster, toSummonBlock, "Biblop Cocco");
        else if (e == Element.Earth)
            ut_execute_monsterSummon(caster, toSummonBlock, "Biblop Reginetta");
        else if (e == Element.Fire)
            ut_execute_monsterSummon(caster, toSummonBlock, "Biblop Griotte");
        else if (e == Element.Water)
            ut_execute_monsterSummon(caster, toSummonBlock, "Biblop Indigo");
    }


    public static void EXECUTE_BLOPPY_HUNGRY(Character caster) {
        if (!put_CheckArguments(new System.Object[] { caster })) return;
        caster.incrementPM(2);
    }

    public static void EXECUTE_ALPHA_TEETH(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (targetBlock.linkedObject == null) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new AlphaTeethEvent("Alpha Teeth", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void SUMMONS_CROBAK_DISTANCE(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Block toSummonBlock = null;
        List<Block> frees = targetBlock.getFreeAdjacentBlocks();
        if (frees.Count == 0) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int indexResult = UnityEngine.Random.Range(0, frees.Count);
        toSummonBlock = frees[indexResult];
        ut_execute_monsterSummon(caster, toSummonBlock, "Little Crobak");
    }

    public static void SUMMONS_AGGR_ARACHNEE_DISTANCE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Block toSummonBlock = null;
        List<Block> frees = targetBlock.getFreeAdjacentBlocks();
        if (frees.Count == 0) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int indexResult = UnityEngine.Random.Range(0, frees.Count);
        toSummonBlock = frees[indexResult];
        ut_execute_monsterSummon(caster, toSummonBlock, "Aggressive Arachnee");
    }

    public static void EXECUTE_MY_CROBAK_CHILDS(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks()))
        {
            ut_execute_monsterSummon(caster, free, "Little Crobak");
        }
    }

    public static void EXECUTE_HIGH_TEMPERATURE(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (targetBlock.linkedObject == null) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new HighTemperatureEvent("High Temperature", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
    }

    public static void EXECUTE_BLOCKING_SPIT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (targetBlock.linkedObject == null) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new BlockingSpitEvent("Blocking Spit", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_WOLF_CRY(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getAlliesWithCaster(caster))
            if (ut_isNearOf(enemy, caster, 3)) {
                WolfCryEvent wce = new WolfCryEvent("Wolf Cry", enemy, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
                enemy.addEvent(wce);
                if (enemy.Equals(caster))
                    wce.useIstantanely();
            }
    }

    public static void EXECUTE_CHIEF_CROCORAGE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getAlliesWithCaster(caster))
            if (ut_isNearOf(enemy, caster, 3)) {
                CrocorageChiefEvent wce = new CrocorageChiefEvent("Crocorage Chief", enemy, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
                enemy.addEvent(wce);
                if (enemy.Equals(caster))
                    wce.useIstantanely();
            }
    }

    public static void EXECUTE_ARACHNEE_EXPLOSION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        List<MonsterEvocation> toExplode = new List<MonsterEvocation>();
        foreach (Character enemy in ut_getAllies(caster))
            if (enemy is Monster) {
                Monster m = (Monster)enemy;
                if (m.monsterSummons.Count > 0) {
                    foreach (MonsterEvocation evoc in m.monsterSummons) {
                        if (evoc.getCompleteName().ToLower().Contains("arach"))
                            toExplode.Add(evoc);
                    }
                }
            }
        foreach (Character hero in ut_getEnemies(caster)) {
            foreach (MonsterEvocation mev in toExplode) {
                if (ut_isNearOf(hero, mev, 2)) {
                    hero.inflictDamage(Spell.calculateDamage(caster, hero, s));
                }
            }
        }
        foreach (MonsterEvocation mev in toExplode) {
            mev.inflictDamage(mev.getActualHP() + mev.actual_shield);
        }
    }

    public static void EXECUTE_HEALING_PLANT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getAlliesWithCaster(caster))
            enemy.receiveHeal(s.damage);
    }


    public static void EXECUTE_CROCORAGE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        CrocorageEvent wce = new CrocorageEvent("Crocorage", caster, s.effectDuration, ParentEvent.Mode.PermanentAndEachTurn, s.icon);
        caster.addEvent(wce);
        wce.useIstantanely();
    }

    public static void EXECUTE_VILINSLASH(Character caster, Block targetBlock, Spell s) {
        ut_damageInLine(caster, targetBlock, s, 2);
    }

    public static void EXECUTE_WAX_SHOT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (targetBlock.linkedObject == null) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new WaxShotEvent("Wax Shot", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        foreach (Character c in ut_getAllies(target)) {
            if (ut_isNearOf(target, c, 2)) {
                c.inflictDamage(Spell.calculateDamage(caster, c, s));
                c.addEvent(new WaxShotEvent("Wax Shot", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
            }
        }
    }

    public static void EXECUTE_EVOKILLER(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (targetBlock.linkedObject == null) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target is Evocation) {
            target.inflictDamage(target.getActualHP() + target.actual_shield);
        } else {
            target.inflictDamage(Spell.calculateDamage(caster, target, s));
        }
    }

    public static void EXECUTE_NINJAWAX(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        if (targetBlock.linkedObject == null) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character c in ut_getAllies(target)) {
            if (ut_isNearOf(target, c, 3)) {
                c.inflictDamage(Spell.calculateDamage(caster, c, s));
            }
        }
    }

    public static void EXECUTE_INSECT_CRY(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new InsectCryEvent("Insect Cry", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon));
    }
    public static void EXECUTE_DEVIL_HUNGRY(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach(Character c in ut_getAlliesWithCaster(caster)) {
            DevilHungryEvent e = new DevilHungryEvent("Devil Hungry", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            c.addEvent(e);
            if (c.Equals(caster))
                e.useIstantanely();
        }
    }

    public static void EXECUTE_HIDEOUT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        HideoutEvent he = new HideoutEvent("Hideout", caster, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon);
        caster.addEvent(he);
        he.useIstantanely();
    }

    public static void EXECUTE_HARD_BONE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAlliesWithCaster(caster)) {
            if (ut_isNearOf(caster, c, 4)) {
                HardBoneEvent hbe = new HardBoneEvent("Hard Bone", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
                c.addEvent(hbe);
                hbe.useIstantanely();
            }
        }
    }

    public static void EXECUTE_NELWEEN_POWER(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster)) {
            NelweenPowerEvent hbe = new NelweenPowerEvent("Nelween Power", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            c.addEvent(hbe);
        }
    }
    public static void EXECUTE_SURVARMOR(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAllies(caster)) {
            SurvarmorEvent hbe = new SurvarmorEvent("Survarmor", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            c.addEvent(hbe);
            hbe.useIstantanely();
        }
    }

    public static void EXECUTE_KANKENSWIFT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 1);
    }

    public static void EXECUTE_SWINGEWL(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 3);
    }

    public static void EXECUTE_KANKENDUST(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, caster.connectedCell.GetComponent<Block>().coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        ut_execute_monsterSummon(caster, toSummonBlock, "Dustmight");
    }

    public static void EXECUTE_MUD_THROW(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, caster.connectedCell.GetComponent<Block>().coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        ut_execute_monsterSummon(caster, toSummonBlock, "Boo");
    }

    public static void EXECUTE_AX_OF_THE_VALKYRIE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        foreach (Character c in ut_getAdjacentHeroes(caster.connectedCell.GetComponent<Block>().coordinate)) {
            if (c.isEnemyOf(caster)) {
                c.inflictDamage(calculateDamage(caster, c, s));
            }
        }
    }

    public static void EXECUTE_SPAROBOOM(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character enemy in ut_getEnemies(caster))
            if (ut_isNearOf(caster, enemy, 2)) {
                enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
            }
        caster.inflictDamage(caster.actual_hp + caster.actual_shield);
    }

    public static void EXECUTE_DESTRUCTION_SWORD(Character caster, Block targetBlock, Spell s) {
        ut_damageInLine(caster, targetBlock, s, 2);
    }

    public static void EXECUTE_FATE_OF_LIGHT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getEnemies(caster)) {
            c.inflictDamage(calculateDamage(caster, c, s));
        }
    }

    public static void EXECUTE_LIGHTS_OUT(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster)) {
            LightsOutEvent me = new LightsOutEvent("Lights Out", enemy, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
            enemy.addEvent(me);
            me.useIstantanely();
        }
    }

    public static void EXECUTE_PSYCHO_ANALYSIS(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster)) {
            enemy.addEvent(new PsychoAnalysisEvent("Psycho Analysis", enemy, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
            enemy.inflictDamage(calculateDamage(caster, enemy, s));
        }
    }

    public static void EXECUTE_BIRTH(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Monster casterMonster = (Monster)caster;
        Character closest = casterMonster.getClosestEnemy();
        if (closest == null) return;
        Block closestBlock = closest.connectedCell.GetComponent<Block>();
        Block toSummonBlock = null;
        int bestDistance = 10000;
        foreach (Block free in (targetBlock.getFreeAdjacentBlocks())) {
            int actualDistance = Monster.getDistance(free.coordinate, closestBlock.coordinate);
            if (actualDistance < bestDistance) {
                bestDistance = actualDistance;
                toSummonBlock = free;
            }
        }
        if (toSummonBlock == null) return;
        ut_execute_monsterSummon(caster, toSummonBlock, "Black Scaraleaf");
    }

    public static void EXECUTE_POISONED_FOG(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.inflictDamage(calculateDamage(caster, target, s));
        target.addEvent(new PoisonedFogEvent("Poisoned Fog", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
        foreach (Character enemy in ut_getAllies(target)) {
            if (ut_isNearOf(enemy, target, 3)) {
                enemy.inflictDamage(calculateDamage(caster, target, s));
                enemy.addEvent(new PoisonedFogEvent("Poisoned Fog", enemy, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
            }
        }
    }

    public static void EXECUTE_CHAFER_WINDSHOT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 5);
    }

    public static void EXECUTE_MUD_SHOT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 3);
    }

    public static void EXECUTE_JUNIOR_BWORK_DISTANCE(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 5);
    }

    public static void EXECUTE_CHAFER_LANCE_EXPLOSION(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster))
        {
            if (ut_isNearOf(enemy, caster, 3))
            {
                enemy.inflictDamage(calculateDamage(caster, enemy, s));
            }
        }
    }
    public static void EXECUTE_LEGENDAIRE_PUNCH(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster))
        {
            if (ut_isNearOf(enemy, caster, 2))
            {
                enemy.inflictDamage(calculateDamage(caster, enemy, s));
            }
        }
    }

    public static void EXECUTE_ROCK_SHOT(Character caster, Block targetBlock, Spell s)
    {
        ut_damageInLine(caster, targetBlock, s, 3);
    }


    public static void EXECUTE_BOTTABOULE(Character caster, Block targetBlock, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        target.addEvent(new BottabouleEvent("Bottaboule", target, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon));
    }

    public static void EXECUTE_HOLY_CHAFER_SWORD(Character caster, Block targetBlock, Spell s) {
        ut_damageInLine(caster, targetBlock, s, 3);
    }

    public static void EXECUTE_CHAFER_FIRESHOT(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        ChaferFireshotEvent targetevent = new ChaferFireshotEvent("Chafer Fireshot", target, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, true);
        target.addEvent(targetevent);
        targetevent.useIstantanely();
        ChaferFireshotEvent casterevent = new ChaferFireshotEvent("Chafer Fireshot", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon, false);
        caster.addEvent(casterevent);
        casterevent.useIstantanely();
    }

    public static void EXECUTE_KWASMUTATION(Character caster, Spell s) {
        KwasmutationEvent kwe = new KwasmutationEvent("Kwasmutation", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(kwe);
        kwe.useIstantanely();
    }

    public static void EXECUTE_GWANDISOLATION(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 5);
    }

    public static void EXECUTE_MAGIC_POWER(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character c in ut_getAlliesWithCaster(caster)) {
            if (ut_isNearOf(caster, c, 3)) {
                MagicPowerEvent mp = new MagicPowerEvent("Magic Power", c, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
                c.addEvent(mp);
                mp.useIstantanely();
            }
        }
    }

    public static void EXECUTE_ASSAILING_ARROW(Character caster, Spell s)
    {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        AssailingArrowEvent mp = new AssailingArrowEvent("Assailing Arrow", caster, s.effectDuration, ParentEvent.Mode.Permanent, s.icon);
        caster.addEvent(mp);
        mp.useIstantanely();
    }

    public static void SUMMONS_ROGUEBOMB_DISTANCE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Block toSummonBlock = null;
        List<Block> frees = targetBlock.getFreeAdjacentBlocks();
        if (frees.Count == 0) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int indexResult = UnityEngine.Random.Range(0, frees.Count);
        toSummonBlock = frees[indexResult];
        MonsterEvocation bomb = ut_execute_monsterSummon(caster, toSummonBlock, "Rogue Bomb");
        bomb.setBomb(caster, bomb.spells[0]);
    }

    public static void SUMMONS_ROGUEBOMB_EVERYFREE(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        List<Block> frees = targetBlock.getFreeAdjacentBlocks();
        foreach (Block free in frees) {
            MonsterEvocation bomb = ut_execute_monsterSummon(caster, free, "Rogue Bomb");
            bomb.setBomb(caster, bomb.spells[0]);
        }
    }

    public static void EXECUTE_BOMBERMAN(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        // get all bombs from the caster and execute explosion
        List<MonsterEvocation> temp_summons = new List<MonsterEvocation>();
        foreach (MonsterEvocation evoTarget in caster.monsterSummons) {
            SUBEXECUTE_EXPLOSION(evoTarget);
            temp_summons.Add(evoTarget);
        }
        foreach (MonsterEvocation evoTarget in temp_summons) {
            evoTarget.inflictDamage(evoTarget.actual_hp + evoTarget.actual_shield);
        }
    }

    public static void EXECUTE_BOMB_THROW(Character caster, Block targetBlock, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return;
        if (!put_CheckLinkedObject(targetBlock)) return;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        foreach (Character enemy in ut_getAllies(target))
                if (ut_isNearOf(target, enemy, 2))
                    enemy.inflictDamage(Spell.calculateDamage(caster, enemy, s));
    }

    public static void EXECUTE_KANNIDESTRUCTION(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster)) {
            if (ut_isNearOf(enemy, caster, 2)) {
                enemy.inflictDamage(calculateDamage(caster, enemy, s));
            }
        }
    }

    public static void EXECUTE_LONG_TURTLE_TELEPORTATION(Character caster, Block targetBlock, Spell s) {
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

    public static void EXECUTE_LONG_TURTLE_HIT(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_repels(caster, targetBlock, 5);
    }

    public static void EXECUTE_KATANA_LUNGE(Character caster, Block targetBlock, Spell s) {
        List<Character> involved = ut_damageInLine(caster, targetBlock, s, 3);
        foreach (Character c in involved) {
            c.addEvent(new KatanaLungeEvent("Katana Lunge", c, s.effectDuration, ParentEvent.Mode.ActivationEachTurn, s.icon, caster, s));
        }
    }

    public static void EXECUTE_TURTLE_CATCH(Character caster, Block targetBlock) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return;
        ut_attracts(caster, targetBlock, 5);
    }

    public static void EXECUTE_TURTLE_HYPERMOVE(Character caster, Spell s) {
        if (!put_CheckArguments(new System.Object[] { caster, s })) return;
        foreach (Character enemy in ut_getEnemies(caster)) {
            if (ut_isNearOf(enemy, caster, 3)) {
                enemy.inflictDamage(calculateDamage(caster, enemy, s));
            }
        }
    }

    #endregion

    #region EVENT BONUSES

    public static int BONUS_ACCUMULATION = 4;
    public static int BONUS_SMITHEREENS = 18;
    public static int BONUS_WRATH = 120;
    public static int BONUS_FATE = 30;
    public static int BONUS_PUNITIVE_ARROW = 36;
    public static int BONUS_BOW_SKILL = 12;
    public static int BONUS_ABOLITION = 14;
    public static int BONUS_ATONEMENT_ARROW = 36;
    public static int BONUS_DECIMATION = 48;
    public static int BONUS_SHADOWYBEAM = 13;
    public static int BONUS_SHOCK = 43;
    public static int BONUS_LETHAL_ATTACK = 30;
    public static int BONUS_KAMA_THROWING = 18;
    public static int BONUS_CONCENTRATION = 21;
    public static int BONUS_SHUSHU_CUT = 20;
    public static int BONUS_BLOODY_PUNISHMENT = 72;
    public static int BONUS_JUDGEMENT_ARROW = 10;
    public static int BONUS_CAT_RAGE = 26;
    public static int BONUS_WATCHDOG = 8;
    public static int BONUS_BEATEN = 15;
    public static int BONUS_TETANISATION = 10;
    public static int BONUS_CALCANEUS = 6;

    public static int EVENT_BONUS_BASE_DAMAGE(Character caster, Character targetch, Spell s) {
        if (caster.name == "Tristepin" && s.name == "Concentration" && (targetch is Evocation || targetch is MonsterEvocation) && caster.getEventSystem().getEvents("Yop God Status").Count > 0) {
            return BONUS_CONCENTRATION;
        }
        if (caster.name == "Mary" && s.name == "Cat Rage") {
            if (caster.summons.Count > 0) {
                Evocation cat = caster.summons[0];
                if (Monster.getDistance(cat.connectedCell.GetComponent<Block>().coordinate, targetch.connectedCell.GetComponent<Block>().coordinate) == 1)
                    return BONUS_CONCENTRATION;
            }
            return 0;
        }
        if (caster.name == "Tristepin" && s.name == "Shushu Cut") {
            int toret = BONUS_SHUSHU_CUT * caster.shushuCounter;
            caster.shushuCounter++;
            return toret;
        }
        if (caster.name == "Missiz Frizz" && s.name == "Accumulation") {
            List<ParentEvent> acclist = caster.getEventSystem().getEvents("Accumulation");
            return BONUS_ACCUMULATION * acclist.Count;
        } else if (caster.name == "Kofang" && s.name == "Smithereens") {
            List<ParentEvent> acclist = caster.getEventSystem().getEvents("Smithereens");
            return BONUS_SMITHEREENS * acclist.Count;
        }
        else if (caster.name == "Vaseky" && s.name == "Abolition Arrow")
        {
            if (targetch is Evocation || targetch is MonsterEvocation)
                return BONUS_ABOLITION;
            else return 0;
        } else if (caster.name == "Robin Blood" && s.name == "Arrow of Judgement") {
            return caster.getActualPM() * BONUS_JUDGEMENT_ARROW;
        } else if (caster.name == "Ragedala" && s.name == "Iop's Wrath")
        {
            List<ParentEvent> acclist = caster.getEventSystem().getEvents("Iop's Wrath");
            return BONUS_WRATH * acclist.Count;
        }
        else if (caster.name == "Gabori" && s.name == "Sword of Fate")
        {
            List<ParentEvent> acclist = caster.getEventSystem().getEvents("Sword of Fate");
            return BONUS_FATE * acclist.Count;
        }
        else if (caster.name == "Volpin" && s.name == "Punitive Arrow")
        {
            List<ParentEvent> acclist = caster.getEventSystem().getEvents("Punitive Arrow");
            return BONUS_PUNITIVE_ARROW * acclist.Count;
        }
        else if (caster.name == "Voldorak" && s.name == "Bow Skill") {
            List<ParentEvent> bslist = caster.getEventSystem().getEvents("Bow Skill");
            return BONUS_BOW_SKILL * bslist.Count;
        } else if (caster.name == "Arc Piven" && s.name == "Atonement Arrow") {
            List<ParentEvent> bslist = caster.getEventSystem().getEvents("Atonement Arrow");
            return BONUS_ATONEMENT_ARROW * bslist.Count;
        } else if (caster.name == "Pilobouli" && s.name == "Decimation") {
            if (caster.hasActivedSacrifice && caster.getTotalHP() * 50 / 100 > caster.getActualHP()) {
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
        }
        else if (caster.name == "Chrona" && s.name == "Shadowy Beam")
        {
            List<Block> adj = targetch.connectedCell.GetComponent<Block>().getFreeAdjacentBlocksWithCharacters(caster.team);
            foreach (Block b in adj)
            {
                if (b.linkedObject != null)
                {
                    Character _c = b.linkedObject.GetComponent<Character>();
                    if (_c != null)
                    {
                        if (_c.team == caster.team)
                            return BONUS_SHADOWYBEAM;
                    }
                }
            }
            return 0;
        } 
        else if (caster.name == "Sanaster" && s.name == "Shock") {
            List<Block> adj = targetch.connectedCell.GetComponent<Block>().getAdjacentBlocks();
            foreach (Block b in adj) {
                if (b.linkedObject != null) {
                    Character _c = b.linkedObject.GetComponent<Character>();
                    if (_c != null) {
                        if (_c is Evocation) {
                            if (((Evocation)_c).isPortal)
                                return BONUS_SHOCK;
                        }
                    }
                }
            }
            return 0;
        } 
        else if (caster.name == "Humawolf" && s.name == "Tetanisation") {
            List<Block> adj = targetch.connectedCell.GetComponent<Block>().getAdjacentBlocks();
            foreach (Block b in adj) {
                if (b.linkedObject != null) {
                    Character _c = b.linkedObject.GetComponent<Character>();
                    if (_c != null) {
                        if (_c is Evocation) {
                            if (((Evocation)_c).isYapper)
                                return BONUS_TETANISATION;
                        }
                    }
                }
            }
            return 0;
        } else if (caster.name == "Groarg Gamel" && s.name == "Beaten") {
            List<Block> adj = targetch.connectedCell.GetComponent<Block>().getAdjacentBlocks();
            foreach (Block b in adj) {
                if (b.linkedObject != null) {
                    Character _c = b.linkedObject.GetComponent<Character>();
                    if (_c != null) {
                        if (_c is Evocation) {
                            if (((Evocation)_c).isYapper)
                                return BONUS_BEATEN;
                        }
                    }
                }
            }
            return 0;
        } else if (caster.name == "Kofang" && s.name == "Watchdog") {
            List<Block> adj = targetch.connectedCell.GetComponent<Block>().getAdjacentBlocks();
            foreach (Block b in adj) {
                if (b.linkedObject != null) {
                    Character _c = b.linkedObject.GetComponent<Character>();
                    if (_c != null) {
                        if (_c is Evocation) {
                            if (((Evocation)_c).isYapper)
                                return BONUS_WATCHDOG;
                        }
                    }
                }
            }
            return 0;
        } else if (caster.name == "Aki" && s.name == "Calcaneus") {
            List<Block> adj = targetch.connectedCell.GetComponent<Block>().getAdjacentBlocks();
            foreach (Block b in adj) {
                if (b.linkedObject != null) {
                    Character _c = b.linkedObject.GetComponent<Character>();
                    if (_c != null) {
                        if (_c is Evocation) {
                            if (((Evocation)_c).isYapper)
                                return BONUS_CALCANEUS;
                        }
                    }
                }
            }
            return 0;
        } else if (caster.name == "Furiado" && s.name == "Bloody Punishment")
        {
            if (caster.actual_hp < targetch.actual_hp)
                return BONUS_BLOODY_PUNISHMENT;
            return 0;
        }
        else if (caster.name == "Etraggy" && s.name == "Lethal Attack") {
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
    public static List<Character> ut_damageInLine(Character caster, Block targetBlock, Spell s, int numberOfCells, bool mustShotAllies = false) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock, s })) return null;
        Coordinate casterCoord = caster.connectedCell.GetComponent<Block>().coordinate;
        Coordinate targetCoord = targetBlock.coordinate;
        if (targetBlock.linkedObject == null) return null;
        Character target = targetBlock.linkedObject.GetComponent<Character>();
        if (target == null) return null;
        List<Character> toShot = ut_getAllies(target);
        List<Character> shotted = new List<Character>();
        if (mustShotAllies) toShot.AddRange(ut_getEnemies(target));
        if (casterCoord.row == targetCoord.row && casterCoord.column < targetCoord.column) {
            // attacking from left
            foreach (Character enemy in toShot) {
                Block enemyBlock = enemy.connectedCell.GetComponent<Block>();
                if (enemyBlock.coordinate.row == targetCoord.row && enemyBlock.coordinate.column > targetCoord.column && enemyBlock.coordinate.column <= targetCoord.column + numberOfCells) {
                    shotted.Add(enemy);
                    enemy.inflictDamage(calculateDamage(caster, enemy, s));
                }
            }
        } else if (casterCoord.row == targetCoord.row && casterCoord.column > targetCoord.column) {
            // attacking from right
            foreach (Character enemy in toShot) {
                Block enemyBlock = enemy.connectedCell.GetComponent<Block>();
                if (enemyBlock.coordinate.row == targetCoord.row && enemyBlock.coordinate.column < targetCoord.column && enemyBlock.coordinate.column >= targetCoord.column - numberOfCells) {
                    shotted.Add(enemy);
                    enemy.inflictDamage(calculateDamage(caster, enemy, s));
                }
            }
        } else if (casterCoord.column == targetCoord.column && casterCoord.row > targetCoord.row) {
            // attacking from bottom
            foreach (Character enemy in toShot) {
                Block enemyBlock = enemy.connectedCell.GetComponent<Block>();
                if (enemyBlock.coordinate.column == targetCoord.column && enemyBlock.coordinate.row < targetCoord.row && enemyBlock.coordinate.row >= targetCoord.row - numberOfCells) {
                    shotted.Add(enemy);
                    enemy.inflictDamage(calculateDamage(caster, enemy, s));
                }
            }
        } else if (casterCoord.column == targetCoord.column && casterCoord.row < targetCoord.row) {
            // attacking from top
            foreach (Character enemy in toShot) {
                Block enemyBlock = enemy.connectedCell.GetComponent<Block>();
                if (enemyBlock.coordinate.column == targetCoord.column && enemyBlock.coordinate.row > targetCoord.row && enemyBlock.coordinate.row <= targetCoord.row + numberOfCells) {
                    shotted.Add(enemy);
                    enemy.inflictDamage(calculateDamage(caster, enemy, s));
                }
            }
        }
        shotted.Add(target);
        return shotted;
    }

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

    public static Evocation ut_execute_summon(Character caster, Block targetBlock, string id, int summonLevel) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return null;
        if (caster.summons.Count == caster.numberOfSummons) return null;
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
        summonScript.injectPowerUp(DUNSelectionManager.UPGRADE, summonLevel); // summonLevel can be 1 OR 2 OR 3
        caster.summons.Add(summonScript);
        // Setting turns parameters
        TurnsManager.Instance.injectCharacter(caster, summonScript);
        return summonScript;
    }

    public static MonsterEvocation ut_execute_monsterSummon(Character caster, Block targetBlock, string id) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return null;
        GameObject summonPrefab = Resources.Load("Prefabs/Monsters/Evocations/" + id) as GameObject;
        // Creating summon
        GameObject summon = GameObject.Instantiate(summonPrefab, Coordinate.getPosition(targetBlock.coordinate), Quaternion.identity);
        // Placing it on field
        summon.transform.position = new Vector3(summon.transform.position.x, summon.transform.position.y, -20);
        targetBlock.linkedObject = summon;
        PreparationManager.Instance.setStandManually(summon, caster.team);
        // Setting summon parameters
        MonsterEvocation summonScript = summon.GetComponent<MonsterEvocation>();
        summonScript.isEvocation = true;
        summonScript.id = caster.summonsIdCounter;
        caster.summonsIdCounter++;
        summonScript.team = caster.team;
        summonScript.connectedSummoner = caster;
        summonScript.connectedCell = targetBlock.gameObject;
        summonScript.setZIndex(targetBlock);
        caster.monsterSummons.Add(summonScript);
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

    public static Block ut_repels(Character caster, Block targetBlock, int numberOfCellsToMove) {
        if (!put_CheckArguments(new System.Object[] { caster, targetBlock })) return null;
        if (!put_CheckLinkedObject(targetBlock)) return null;
        if (!targetBlock.linkedObject.GetComponent<Character>().canMovedByEffects) return null;
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
        {
            targetBlock.linkedObject.GetComponent<Character>().setPath(path); // move the enemy
            return path[path.Count - 1]; // returning destination
        }
        return null;
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

    public static int ut_getAlliedPortalPMGains(Character actual) {
        List<Character> chs = ut_getAllies(actual);
        int counter = 0;
        foreach (Character ch in chs) {
            if (ch is Evocation) {
                Evocation e = (Evocation)ch;
                if (e.isPortal) {
                    if (ut_isNearOf(e, actual, 3)) {
                        counter++;
                    }
                }
            }
        }
        return counter;
    }
    public static int ut_getAlliedPortals(Character actual) {
        List<Character> chs = ut_getAllies(actual);
        int counter = 0;
        foreach (Character ch in chs) {
            if (ch is Evocation) {
                Evocation e = (Evocation)ch;
                if (e.isPortal) {
                    counter++;
                }
            }
        }
        return counter;
    }

    #endregion
}