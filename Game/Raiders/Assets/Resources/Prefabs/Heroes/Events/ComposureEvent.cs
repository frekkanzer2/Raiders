using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComposureEvent : ParentEvent
{

    public ComposureEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.res_w -= 6;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_w += 6;
    }

}
