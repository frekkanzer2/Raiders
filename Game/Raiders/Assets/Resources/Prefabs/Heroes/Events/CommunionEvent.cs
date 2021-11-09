using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunionEvent : ParentEvent
{

    public CommunionEvent(string name, Evocation c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        ((Evocation)connected).isCommunionActive = true;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        ((Evocation)connected).isCommunionActive = false;
    }

}
