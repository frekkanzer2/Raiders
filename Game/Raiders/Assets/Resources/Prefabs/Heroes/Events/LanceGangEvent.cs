using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanceGangEvent : ParentEvent
{

    private int bonus = 0;

    public LanceGangEvent(string name, Character c, int duration, Mode mode, Sprite s, int bonus) : base(name, c, duration, mode, s) { this.bonus = bonus; }

    override public void execute() {
        base.execute();
        connected.att_a += bonus;
        connected.att_e += bonus;
        connected.att_w += bonus;
        connected.att_f += bonus;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= bonus;
        connected.att_e -= bonus;
        connected.att_w -= bonus;
        connected.att_f -= bonus;
    }

}
