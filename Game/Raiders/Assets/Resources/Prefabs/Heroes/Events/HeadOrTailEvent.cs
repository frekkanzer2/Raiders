using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadOrTailEvent : ParentEvent
{

    Character caster;

    public HeadOrTailEvent(string name, Character c, int duration, Mode mode, Sprite s, Character caster) : base(name, c, duration, mode, s) { this.caster = caster; }

    override public void execute() {
        base.execute();
        Random.InitState((int)System.DateTime.Now.Ticks);
	    if (UnityEngine.Random.Range(1, 101) <= 50)
            connected.receiveHeal(40 + caster.bonusHeal);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
