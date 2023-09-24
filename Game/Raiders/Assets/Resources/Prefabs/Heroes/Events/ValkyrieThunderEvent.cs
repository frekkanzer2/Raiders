using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValkyrieThunderEvent : ParentEvent
{

    public ValkyrieThunderEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 25;
        connected.att_e += 25;
        connected.att_w += 25;
        connected.att_f += 25;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 25;
        connected.att_e -= 25;
        connected.att_w -= 25;
        connected.att_f -= 25;
    }

}
