using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortugaEvent : ParentEvent
{

    Character caster;

    public TortugaEvent(string name, Character c, int duration, Mode mode, Sprite s, Character caster) : base(name, c, duration, mode, s) { this.caster = caster;  }

    override public void execute() {
        base.execute();
        connected.receiveShield(80 + caster.bonusGainShield);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.removeShield(80 + caster.bonusGainShield);
    }

}
