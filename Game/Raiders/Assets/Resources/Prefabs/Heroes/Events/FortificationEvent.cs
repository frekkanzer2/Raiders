using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortificationEvent : ParentEvent
{

    Character caster;

    public FortificationEvent(string name, Character c, int duration, Mode mode, Sprite s, Character caster) : base(name, c, duration, mode, s) { this.caster = caster; }

    override public void execute() {
        base.execute();
        connected.receiveShield(40 + caster.bonusGainShield);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
