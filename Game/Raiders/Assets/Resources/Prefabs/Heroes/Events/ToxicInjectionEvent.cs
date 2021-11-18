using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicInjectionEvent : ParentEvent
{

    public Spell referencedSpell;

    public ToxicInjectionEvent(string name, Character c, int duration, Mode mode, Sprite s, Spell spell) : base(name, c, duration, mode, s) { this.referencedSpell = spell; }

    override public void execute() {
        base.execute();

    }

    override public void restoreCharacter() {
        base.restoreCharacter();

    }

}
