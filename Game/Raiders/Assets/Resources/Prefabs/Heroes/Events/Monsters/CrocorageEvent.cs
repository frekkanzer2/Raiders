using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrocorageEvent : ParentEvent
{

    public CrocorageEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        connected.att_a += 25;
        connected.att_e += 25;
        connected.att_w += 25;
        connected.att_f += 25;
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.incrementPM(3);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 25;
        connected.att_e -= 25;
        connected.att_w -= 25;
        connected.att_f -= 25;
    }

}
