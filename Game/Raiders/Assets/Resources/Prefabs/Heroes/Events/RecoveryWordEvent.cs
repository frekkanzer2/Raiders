using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoveryWordEvent : ParentEvent
{

    public RecoveryWordEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.decrementPM(100);
        connected.decrementPA(100);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
