using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsychopathMask : ParentEvent
{

    public PsychopathMask(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        connected.att_a += 80;
        connected.att_e += 80;
        connected.att_w += 80;
        connected.att_f += 80;
    }

    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.decrementPM(1);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= 80;
        connected.att_e -= 80;
        connected.att_w -= 80;
        connected.att_f -= 80;
    }

}
