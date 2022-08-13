using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackRatOverhitEvent : ParentEvent
{

    public BlackRatOverhitEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.res_e -= 20;
        connected.res_w -= 20;
        connected.res_f -= 20;
        connected.res_a -= 20;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_e += 20;
        connected.res_w += 20;
        connected.res_f += 20;
        connected.res_a += 20;
    }

}
