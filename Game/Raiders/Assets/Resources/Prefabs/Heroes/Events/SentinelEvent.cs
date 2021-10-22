using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentinelEvent : ParentEvent
{

    public SentinelEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 40;
        connected.att_e += 40;
        connected.att_w += 40;
        connected.att_f += 40;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 40;
        connected.att_e -= 40;
        connected.att_w -= 40;
        connected.att_f -= 40;
    }

}
