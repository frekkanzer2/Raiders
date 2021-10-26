using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmellEvent : ParentEvent
{

    public SmellEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        connected.res_a -= 25;
        connected.res_e -= 25;
        connected.res_w -= 25;
        connected.res_f -= 25;
    }
    
    override public void both_newTurnExecute() {
	    base.both_newTurnExecute();
	    UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
	    if (UnityEngine.Random.Range(1, 101) <= 50)
            connected.incrementPA(2);
        else connected.incrementPM(2);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.res_a += 25;
        connected.res_e += 25;
        connected.res_w += 25;
        connected.res_f += 25;
    }

}
