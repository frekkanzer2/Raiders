using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompulsionEvent : ParentEvent
{

    public CompulsionEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 6;
        connected.att_e += 6;
        connected.att_w += 6;
        connected.att_f += 6;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 6;
        connected.att_e -= 6;
        connected.att_w -= 6;
        connected.att_f -= 6;
    }

}
