using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuriousWordEvent : ParentEvent
{

    public FuriousWordEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 100;
        connected.att_e += 100;
        connected.att_w += 100;
        connected.att_f += 100;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 100;
        connected.att_e -= 100;
        connected.att_w -= 100;
        connected.att_f -= 100;
    }

}
