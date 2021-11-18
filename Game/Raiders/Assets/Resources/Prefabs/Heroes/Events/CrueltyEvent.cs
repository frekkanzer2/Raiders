using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrueltyEvent : ParentEvent
{

    bool res = false;

    public CrueltyEvent(string name, Character c, int duration, Mode mode, Sprite s, bool mustRemove) : base(name, c, duration, mode, s) { this.res = mustRemove; }

    override public void execute() {
        base.execute();
        if (res)
            connected.decrementPM(1);
        else
            connected.incrementPM(1);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
