using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CriticalShootingEvent : ParentEvent
{

    public CriticalShootingEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.criticShooting = true;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.criticShooting = false;
    }

}
