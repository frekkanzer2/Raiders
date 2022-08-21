using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrocorageChiefEvent : ParentEvent
{

    public CrocorageChiefEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        connected.att_a += 30;
        connected.att_e += 30;
        connected.att_w += 30;
        connected.att_f += 30;
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.incrementPM(2);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 30;
        connected.att_e -= 30;
        connected.att_w -= 30;
        connected.att_f -= 30;
    }

}
