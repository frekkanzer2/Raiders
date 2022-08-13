using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteRatOverhitEvent : ParentEvent
{

    public WhiteRatOverhitEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.res_e -= 40;
        connected.res_w -= 40;
        connected.res_f -= 40;
        connected.res_a -= 40;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_e += 40;
        connected.res_w += 40;
        connected.res_f += 40;
        connected.res_a += 40;
    }

}
