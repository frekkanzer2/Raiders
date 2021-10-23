using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkEvent : ParentEvent
{

    private int percBonus = 0;

    public BerserkEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        this.percBonus = 100 - (connected.actual_hp * 100 / connected.hp);
        connected.att_a += percBonus;
        connected.att_e += percBonus;
        connected.att_w += percBonus;
        connected.att_f += percBonus;
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.actual_pa += 2;
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.att_a -= percBonus;
        connected.att_e -= percBonus;
        connected.att_w -= percBonus;
        connected.att_f -= percBonus;
    }

}
