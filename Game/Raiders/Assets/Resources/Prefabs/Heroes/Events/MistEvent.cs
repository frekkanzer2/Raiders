using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistEvent : ParentEvent
{

    public MistEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.res_e -= 25;
        connected.res_w -= 25;
        connected.res_f -= 25;
        connected.res_a -= 25;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_e += 25;
        connected.res_w += 25;
        connected.res_f += 25;
        connected.res_a += 25;
    }

}
