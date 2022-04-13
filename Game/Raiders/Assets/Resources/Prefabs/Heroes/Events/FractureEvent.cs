using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractureEvent : ParentEvent
{

    public FractureEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.res_a -= 7;
        connected.res_e -= 7;
        connected.res_w -= 7;
        connected.res_f -= 7;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_a += 7;
        connected.res_e += 7;
        connected.res_w += 7;
        connected.res_f += 7;
    }

}
