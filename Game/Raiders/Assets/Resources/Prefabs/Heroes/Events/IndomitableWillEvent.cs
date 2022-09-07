using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndomitableWillEvent : ParentEvent
{

    public IndomitableWillEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
