using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionGlyphEvent : ParentEvent
{

    Character caster = null;
    Spell launched = null;

    public PerceptionGlyphEvent(string name, Character c, int duration, Mode mode, Sprite s, Character caster, Spell launched) : base(name, c, duration, mode, s) { this.caster = caster; this.launched = launched; }

    override public void execute() {
        base.execute();
        connected.canCritical = false;
        connected.inflictDamage(Spell.calculateDamage(caster, connected, launched));
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.canCritical = true;
    }

}
