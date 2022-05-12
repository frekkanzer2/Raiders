using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructiveArrowEvent : ParentEvent
{

    public DestructiveArrowEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 10;
        connected.att_e += 10;
        connected.att_w += 10;
        connected.att_f += 10;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 10;
        connected.att_e -= 10;
        connected.att_w -= 10;
        connected.att_f -= 10;
    }

}
