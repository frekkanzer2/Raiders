using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepulsionGlyphEvent : ParentEvent
{

    public RepulsionGlyphEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        List<Character> toMove = Spell.ut_getAdjacentHeroes(connected.connectedCell.GetComponent<Block>().coordinate);
        foreach (Character c in toMove) {
            if (c.isEnemyOf(connected))
                Spell.ut_repels(connected, c.connectedCell.GetComponent<Block>(), 5);
        }
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
