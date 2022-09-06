using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvarmorEvent : ParentEvent
{

    public SurvarmorEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.res_a += 10;
        connected.res_e += 10;
        connected.res_w += 10;
        connected.res_f += 10;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_a -= 10;
        connected.res_e -= 10;
        connected.res_w -= 10;
        connected.res_f -= 10;
    }

}
