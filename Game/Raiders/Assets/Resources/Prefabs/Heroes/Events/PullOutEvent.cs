using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PullOutEvent : ParentEvent
{

    public PullOutEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        if (remainingTurns == 2)
            foreach (Character enemy in Spell.ut_getEnemies(connected))
                enemy.decrementPM(100);
        if (remainingTurns == 1)
            connected.decrementPM(100);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
