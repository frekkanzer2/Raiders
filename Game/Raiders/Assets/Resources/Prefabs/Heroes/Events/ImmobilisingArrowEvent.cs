using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmobilisingArrowEvent : ParentEvent
{

    public ImmobilisingArrowEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.decrementPM(1);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
