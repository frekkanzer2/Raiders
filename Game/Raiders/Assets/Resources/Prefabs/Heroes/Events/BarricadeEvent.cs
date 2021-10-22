using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarricadeEvent : ParentEvent
{

    public BarricadeEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.actual_pm -= 1;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}