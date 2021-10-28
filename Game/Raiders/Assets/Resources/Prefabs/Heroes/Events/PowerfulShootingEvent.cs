using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerfulShooting : ParentEvent
{

    public PowerfulShooting(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.att_a += 150;
        connected.att_e += 150;
        connected.att_w += 150;
        connected.att_f += 150;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 150;
        connected.att_e -= 150;
        connected.att_w -= 150;
        connected.att_f -= 150;
    }

}
