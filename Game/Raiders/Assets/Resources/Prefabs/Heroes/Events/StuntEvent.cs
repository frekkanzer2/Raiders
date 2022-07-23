using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StuntEvent : ParentEvent
{

    public StuntEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 80;
        connected.att_e += 80;
        connected.att_w += 80;
        connected.att_f += 80;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 80;
        connected.att_e -= 80;
        connected.att_w -= 80;
        connected.att_f -= 80;
    }

}
