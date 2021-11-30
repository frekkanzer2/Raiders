using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObsolescenceEvent : ParentEvent
{

    public ObsolescenceEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.decrementPM(1);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
