using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnconsciousCombatEvent : ParentEvent
{

    public UnconsciousCombatEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 120;
        connected.att_e += 120;
        connected.att_w += 120;
        connected.att_f += 120;
        connected.res_a -= 40;
        connected.res_e -= 40;
        connected.res_w -= 40;
        connected.res_f -= 40;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_a += 40;
        connected.res_e += 40;
        connected.res_w += 40;
        connected.res_f += 40;
        connected.att_a -= 120;
        connected.att_e -= 120;
        connected.att_w -= 120;
        connected.att_f -= 120;
    }

}
