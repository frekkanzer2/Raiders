using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolboxEvent : ParentEvent
{

    public ToolboxEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.incrementPA(3);
        connected.decrementPM(100);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
