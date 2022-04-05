using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Upgrade
{

    public int availablePoints = 0; // DO NOT EDIT THE VALUE OF THIS VAR!!!
    public int pointsToAssign = 0;

    public int hpLevel = 0;
    public int initLevel = 0;
    public int pmLevel = 0; // max 2 PM - 100 pts
    public int paLevel = 0; // max 2 PA - 120 pts
    public int atkEarthLevel = 0;
    public int atkFireLevel = 0;
    public int atkAirLevel = 0;
    public int atkWaterLevel = 0;
    public int allAtkLevel = 0;
    public int defEarthLevel = 0; // max 30 - 30 pts
    public int defFireLevel = 0; // max 30 - 30 pts
    public int defAirLevel = 0; // max 30 - 30 pts
    public int defWaterLevel = 0; // max 30 - 30 pts
    public int allDefLevel = 0; // max 20 - 40 pts
    public int evocationLevel = 0; // max 2 SUMMONS - 80 pts
    public int startingShield = 0;

    #region Execute Powerups

    public bool executePowerup_hpLevel(int variation) {
        Debug.Log("Variation " + variation + " with points to assign " + pointsToAssign);
        if (variation > 0) {
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                Debug.Log("Assigning HP points");
                pointsToAssign -= variation;
                hpLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (hpLevel >= Math.Abs(variation)) { // change here
                hpLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_startingShield(int variation) {
        if (variation > 0) {
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                startingShield += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (startingShield >= Math.Abs(variation)) { // change here
                startingShield += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_initLevel(int variation) {
        if (variation > 0) {
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                initLevel += variation;
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (initLevel >= Math.Abs(variation)) {
                initLevel += variation;
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_pmLevel(int variation) {
        if (variation > 0) {
            if (variation + pmLevel > 100) return false;
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                pmLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (pmLevel >= Math.Abs(variation)) { // change here
                pmLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_paLevel(int variation) {
        if (variation > 0) {
            if (variation + paLevel > 100) return false;
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                paLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (paLevel >= Math.Abs(variation)) { // change here
                paLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_atkEarthLevel(int variation) {
        if (variation > 0) {
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                atkEarthLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (atkEarthLevel >= Math.Abs(variation)) { // change here
                atkEarthLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_atkFireLevel(int variation) {
        if (variation > 0) {
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                atkFireLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (atkFireLevel >= Math.Abs(variation)) { // change here
                atkFireLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_atkAirLevel(int variation) {
        if (variation > 0) {
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                atkAirLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (atkAirLevel >= Math.Abs(variation)) { // change here
                atkAirLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_atkWaterLevel(int variation) {
        if (variation > 0) {
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                atkWaterLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (atkWaterLevel >= Math.Abs(variation)) { // change here
                atkWaterLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_allAtkLevel(int variation) {
        if (variation > 0) {
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                allAtkLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (allAtkLevel >= Math.Abs(variation)) { // change here
                allAtkLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_allDefLevel(int variation) {
        if (variation > 0) {
            if (variation + allDefLevel > 20) return false;
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                allDefLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (allDefLevel >= Math.Abs(variation)) { // change here
                allDefLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_defEarthLevel(int variation) {
        if (variation > 0) {
            if (variation + defEarthLevel > 30) return false;
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                defEarthLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (defEarthLevel >= Math.Abs(variation)) { // change here
                defEarthLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_defFireLevel(int variation) {
        if (variation > 0) {
            if (variation + defFireLevel > 30) return false;
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                defFireLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (defFireLevel >= Math.Abs(variation)) { // change here
                defFireLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_defAirLevel(int variation) {
        if (variation > 0) {
            if (variation + defAirLevel > 30) return false;
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                defAirLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (defAirLevel >= Math.Abs(variation)) { // change here
                defAirLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_defWaterLevel(int variation) {
        if (variation > 0) {
            if (variation + defWaterLevel > 30) return false;
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                defWaterLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (defWaterLevel >= Math.Abs(variation)) { // change here
                defWaterLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }
    public bool executePowerup_evocationLevel(int variation) {
        if (variation > 0) {
            if (variation + evocationLevel > 80) return false;
            // assigning points
            if (variation > pointsToAssign) return false;
            else {
                pointsToAssign -= variation;
                evocationLevel += variation; // change here
                return true;
            }
        } else if (variation < 0) {
            // getting points
            if (evocationLevel >= Math.Abs(variation)) { // change here
                evocationLevel += variation; // change here
                pointsToAssign += Math.Abs(variation);
                return true;
            } else return false;
        } else return false;
    }

    #endregion

    public void resetLevels() {
        hpLevel = 0;
        initLevel = 0;
        pmLevel = 0; // max 2 PM - 100 pts
        paLevel = 0; // max 2 PA - 120 pts
        atkEarthLevel = 0;
        atkFireLevel = 0;
        atkAirLevel = 0;
        atkWaterLevel = 0;
        allAtkLevel = 0;
        defEarthLevel = 0; // max 30 - 30 pts
        defFireLevel = 0; // max 30 - 30 pts
        defAirLevel = 0; // max 30 - 30 pts
        defWaterLevel = 0; // max 30 - 30 pts
        allDefLevel = 0; // max 20 - 40 pts
        evocationLevel = 0; // max 2 SUMMONS - 80 pts
        startingShield = 0;
        pointsToAssign = availablePoints;
    }

    public int getHpBonus() {
        return hpLevel * 5;
    }

    public int getHealBonus() {
        return hpLevel / 2;
    }

    public int getInitBonus() {
        return initLevel * 3;
    }

    public int getPmBonus() {
        return pmLevel / 50;
    }

    public int getPaBonus() {
        return paLevel / 60;
    }

    // Returns Tuple<Earth, Fire, Air, Water>
    public Tuple<int, int, int, int> getAttackBonus() {
        return new Tuple<int, int, int, int>(
            atkEarthLevel * 2 + allAtkLevel/3*2,
            atkFireLevel * 2 + allAtkLevel/3*2,
            atkAirLevel * 2 + allAtkLevel/3*2,
            atkWaterLevel * 2 + allAtkLevel/3*2
        );
    }

    // Returns Tuple<Earth, Fire, Air, Water>
    public Tuple<int, int, int, int> getDefenceBonus() {
        return new Tuple<int, int, int, int>(
            defEarthLevel + allDefLevel / 2,
            defFireLevel + allDefLevel / 2,
            defAirLevel + allDefLevel / 2,
            defWaterLevel + allDefLevel / 2
        );
    }

    public int getSummonsBonus() {
        return evocationLevel / 40;
    }

    public int getShieldBonus() {
        return startingShield * 8;
    }

}
