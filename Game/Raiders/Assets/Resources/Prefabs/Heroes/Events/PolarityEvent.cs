using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolarityEvent : ParentEvent
{

    int powerup = 0;

    public PolarityEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        Coordinate connectedCoord = connected.connectedCell.GetComponent<Block>().coordinate;
        foreach (Character ally in Spell.ut_getAllies(connected)) {
            Coordinate allyCoord = ally.connectedCell.GetComponent<Block>().coordinate;
            if (connectedCoord.row == allyCoord.row || connectedCoord.column == allyCoord.column)
                powerup += 30;
        }
        connected.att_a += powerup;
        connected.att_e += powerup;
        connected.att_w += powerup;
        connected.att_f += powerup;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= powerup;
        connected.att_e -= powerup;
        connected.att_w -= powerup;
        connected.att_f -= powerup;
    }

}
