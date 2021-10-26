using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CowardMask : ParentEvent
{

    public CowardMask(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        connected.res_a -= 20;
        connected.res_e -= 20;
        connected.res_w -= 20;
        connected.res_f -= 20;
    }

    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.incrementPM(2);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.res_a += 20;
        connected.res_e += 20;
        connected.res_w += 20;
        connected.res_f += 20;
    }

}
