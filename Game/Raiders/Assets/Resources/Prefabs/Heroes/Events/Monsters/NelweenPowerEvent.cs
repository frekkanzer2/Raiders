using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NelweenPowerEvent : ParentEvent
{

    public NelweenPowerEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 20;
        connected.att_e += 20;
        connected.att_w += 20;
        connected.att_f += 20;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 20;
        connected.att_e -= 20;
        connected.att_w -= 20;
        connected.att_f -= 20;
    }

}
