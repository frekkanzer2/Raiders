using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonerFuryEvent : ParentEvent
{

    public SummonerFuryEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 70;
        connected.att_e += 70;
        connected.att_w += 70;
        connected.att_f += 70;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 70;
        connected.att_e -= 70;
        connected.att_w -= 70;
        connected.att_f -= 70;
    }

}
