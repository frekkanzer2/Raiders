using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfernoEvent : ParentEvent
{

    public InfernoEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 200;
        connected.att_e += 200;
        connected.att_w += 200;
        connected.att_f += 200;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 200;
        connected.att_e -= 200;
        connected.att_w -= 200;
        connected.att_f -= 200;
    }

}
