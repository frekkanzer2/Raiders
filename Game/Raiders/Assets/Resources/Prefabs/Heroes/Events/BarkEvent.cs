using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarkEvent : ParentEvent
{

    public BarkEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a -= 30;
        connected.att_e -= 30;
        connected.att_w -= 30;
        connected.att_f -= 30;
        connected.res_a -= 20;
        connected.res_e -= 20;
        connected.res_w -= 20;
        connected.res_f -= 20;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a += 30;
        connected.att_e += 30;
        connected.att_w += 30;
        connected.att_f += 30;
        connected.res_a += 20;
        connected.res_e += 20;
        connected.res_w += 20;
        connected.res_f += 20;
    }

}
