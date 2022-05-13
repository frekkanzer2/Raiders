using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementalDrainEvent : ParentEvent
{

    public ElementalDrainEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 60;
        connected.att_e += 60;
        connected.att_w += 60;
        connected.att_f += 60;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 60;
        connected.att_e -= 60;
        connected.att_w -= 60;
        connected.att_f -= 60;
    }

}
