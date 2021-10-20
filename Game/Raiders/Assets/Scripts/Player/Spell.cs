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
    public Element element;
    public int damage;
    public bool lifeSteal;
    public int minRange;
    public int maxRange;
    public DistanceType distanceType;
    public bool overObstacles;
    public int hpCost;
    public int paCost;
    public int pmCost;
    public int maxTimesInTurn;
    public int executeAfterTurns;
    public int effectDuration;
    public float criticalProbability; // 0 to 100% -> +20% DMG
    public bool hasEffect;
    public bool isEffectOnly;
    public bool canUseInEmptyCell;

    public void OnPreviewPressed() {
        Debug.Log("Pressed spell " + name);
    }

}
