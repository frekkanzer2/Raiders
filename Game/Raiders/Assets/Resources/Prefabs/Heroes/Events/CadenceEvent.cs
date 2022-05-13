using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CadenceEvent : ParentEvent
{

    int n = 0;
    public CadenceEvent(string name, Character c, int duration, Mode mode, Sprite s, int npm) : base(name, c, duration, mode, s) { n = npm; }

    override public void execute() {
        base.execute();
        connected.decrementPM(n);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
