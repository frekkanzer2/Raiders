using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTheftEvent : ParentEvent
{

    private Character caster;

    public TimeTheftEvent(string name, Character c, int duration, Mode mode, Sprite s, Character launcher) : base(name, c, duration, mode, s) { caster = launcher; }

    override public void execute() {
        base.execute();
	    connected.decrementPA(2);
	    UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        if (UnityEngine.Random.Range(1, 101) <= 80) {
            caster.incrementPA(1);
        }
	    UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        if (UnityEngine.Random.Range(1, 101) <= 25) {
            caster.incrementPA(1);
        }
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
