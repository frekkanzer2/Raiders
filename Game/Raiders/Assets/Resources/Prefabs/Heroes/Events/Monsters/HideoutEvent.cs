using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideoutEvent : ParentEvent
{

    public HideoutEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.incrementPM(3);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
