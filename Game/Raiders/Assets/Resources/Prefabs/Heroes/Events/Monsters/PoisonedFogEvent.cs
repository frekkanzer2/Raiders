using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonedFogEvent : ParentEvent
{

    public PoisonedFogEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.decrementPA(3);
        connected.decrementPM(2);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
