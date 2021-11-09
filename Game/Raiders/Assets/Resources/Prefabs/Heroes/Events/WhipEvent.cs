using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhipEvent : ParentEvent
{

    public WhipEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.incrementPA(1);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
