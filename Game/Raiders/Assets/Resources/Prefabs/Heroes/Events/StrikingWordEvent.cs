using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikingWordEvent : ParentEvent
{

    Character caster;

    public StrikingWordEvent(string name, Character c, int duration, Mode mode, Sprite s, Character caster) : base(name, c, duration, mode, s) { this.caster = caster; }

    override public void execute() {
        base.execute();
	    connected.receiveHeal(30 + caster.bonusHeal);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
