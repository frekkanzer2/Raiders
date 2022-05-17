using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventingWordEvent : ParentEvent
{

    Character caster;

    public PreventingWordEvent(string name, Character c, int duration, Mode mode, Sprite s, Character caster) : base(name, c, duration, mode, s) { this.caster = caster;  }

    override public void execute() {
        base.execute();
        connected.receiveShield(200 + caster.bonusGainShield);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.removeShield(200 + caster.bonusGainShield);
    }

}
