using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectiveGlyphEvent : ParentEvent
{

    int t = -1;
    Character caster;
    Spell spell;

    public ProtectiveGlyphEvent(string name, Character c, int duration, Mode mode, Sprite s, int type, Character caster, Spell sp) : base(name, c, duration, mode, s) { t = type; this.caster = caster; spell = sp; }

    // Hybrid: it depends on Mode passed as argument
    override public void execute() {
        base.execute();
        if (t == 0) {
            // ally
            connected.res_a += 20;
            connected.res_e += 20;
            connected.res_w += 20;
            connected.res_f += 20;
        }
        else if (t == 1) {
            // enemy
            connected.inflictDamage(Spell.calculateDamage(caster, connected, spell));
        }
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        if (t == 0) {
            connected.res_a -= 20;
            connected.res_e -= 20;
            connected.res_w -= 20;
            connected.res_f -= 20;
        }
    }

}
