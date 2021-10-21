using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgitationEvent : ParentEvent
{

    public AgitationEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.actual_pm += 2;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
