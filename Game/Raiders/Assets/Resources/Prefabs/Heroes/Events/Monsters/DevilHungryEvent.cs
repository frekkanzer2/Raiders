using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevilHungryEvent : ParentEvent
{

    public DevilHungryEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 30;
        connected.att_e += 30;
        connected.att_w += 30;
        connected.att_f += 30;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 30;
        connected.att_e -= 30;
        connected.att_w -= 30;
        connected.att_f -= 30;
    }

}
