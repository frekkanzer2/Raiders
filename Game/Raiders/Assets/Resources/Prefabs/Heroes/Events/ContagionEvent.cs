using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContagionEvent : ParentEvent
{

    Character caster = null;
    Spell launched = null;

    public ContagionEvent(string name, Character c, int duration, Mode mode, Sprite s, Character caster, Spell launched) : base(name, c, duration, mode, s) { this.caster = caster; this.launched = launched; }

    override public void execute() {
        base.execute();
        connected.inflictDamage(Spell.calculateDamage(caster, connected, launched), true);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
