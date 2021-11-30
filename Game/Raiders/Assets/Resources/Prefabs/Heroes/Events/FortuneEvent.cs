using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortuneEvent : ParentEvent
{

    public FortuneEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        if (remainingTurns == 2) {
            connected.att_a += 200;
            connected.att_e += 200;
            connected.att_w += 200;
            connected.att_f += 200;
        }
        if (remainingTurns == 1) {
            connected.att_a -= 200;
            connected.att_e -= 200;
            connected.att_w -= 200;
            connected.att_f -= 200;
            connected.decrementPA(100);
        }
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
