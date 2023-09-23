using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroicPowerEvent : ParentEvent
{

    public HeroicPowerEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 25;
        connected.att_e += 25;
        connected.att_w += 25;
        connected.att_f += 25;
        connected.res_a += 10;
        connected.res_e += 10;
        connected.res_w += 10;
        connected.res_f += 10;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 25;
        connected.att_e -= 25;
        connected.att_w -= 25;
        connected.att_f -= 25;
        connected.res_a -= 10;
        connected.res_e -= 10;
        connected.res_w -= 10;
        connected.res_a -= 10;
    }

}
