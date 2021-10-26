using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazinessEvent : ParentEvent
{

    public HazinessEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        if (remainingTurns == 2)
            connected.decrementPA(2);
        if (remainingTurns == 1) {
            connected.incrementPA(2);
            connected.incrementPM(1);
        }
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
