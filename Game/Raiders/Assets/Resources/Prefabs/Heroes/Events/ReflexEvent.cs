using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflexEvent : ParentEvent
{

    public ReflexEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    bool preactivation = false;
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        connected.att_a -= 50;
        connected.att_e -= 50;
        connected.att_w -= 50;
        connected.att_f -= 50;
    }

    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        if (preactivation)
            connected.incrementPM(3);
        else preactivation = true;
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a += 50;
        connected.att_e += 50;
        connected.att_w += 50;
        connected.att_f += 50;
    }

}
