using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelEvent : ParentEvent
{

    public DuelEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.pm -= 100;
        connected.actual_pm -= 100;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.pm += 100;
        connected.actual_pm += 100;
    }

}
