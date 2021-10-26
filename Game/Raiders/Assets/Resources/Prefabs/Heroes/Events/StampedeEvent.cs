using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StampedeEvent : ParentEvent
{

    public StampedeEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.incrementPM(1);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
