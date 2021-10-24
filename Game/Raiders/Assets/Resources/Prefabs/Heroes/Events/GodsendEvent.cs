using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodsendEvent : ParentEvent
{

    public GodsendEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.res_e -= 15;
        connected.res_w -= 15;
        connected.res_f -= 15;
        connected.res_a -= 15;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_e += 15;
        connected.res_w += 15;
        connected.res_f += 15;
        connected.res_a += 15;
    }

}
