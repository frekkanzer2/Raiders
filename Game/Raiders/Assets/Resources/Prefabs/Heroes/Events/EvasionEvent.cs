using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvasionEvent : ParentEvent
{

    int incr_pm = 0;

    public EvasionEvent(string name, Character c, int duration, Mode mode, Sprite s, int increment) : base(name, c, duration, mode, s) { this.incr_pm = increment;  }

    override public void execute() {
        base.execute();
        connected.incrementPM(incr_pm);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
