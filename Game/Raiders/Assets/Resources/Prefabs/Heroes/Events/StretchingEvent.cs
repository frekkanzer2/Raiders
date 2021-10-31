using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StretchingEvent : ParentEvent
{

    public StretchingEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        connected.res_a -= 10;
        connected.res_e -= 10;
        connected.res_w -= 10;
        connected.res_f -= 10;
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.incrementPM(1);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.res_a += 10;
        connected.res_e += 10;
        connected.res_w += 10;
        connected.res_f += 10;
    }

}
