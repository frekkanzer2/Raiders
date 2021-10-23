using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MutilationEvent : ParentEvent
{

    private int gain = 50;

    public MutilationEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        if (connected.actual_hp < connected.hp / 2) gain *= 2;
        connected.att_a += gain;
        connected.att_e += gain;
        connected.att_w += gain;
        connected.att_f += gain;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= gain;
        connected.att_e -= gain;
        connected.att_w -= gain;
        connected.att_f -= gain;
    }

}
