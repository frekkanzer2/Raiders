using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouletteEvent : ParentEvent
{

    private int bonusId = 0;
    // 1: +1 PA
    // 2: +1 PM
    // 3: +2 PA
    // 4: +2 PM
    // 5: +50% dmg e
    // 6: +50% dmg f
    // 7: +50% dmg a
    // 8: +50% dmg w
    // 9: +50% res e
    // 10: +50% res f
    // 11: +50% res a
    // 12: +50% res w
    // 13: +50% dmg
    // 14: +50% res
    // 15: heal of 100 hp

    public RouletteEvent(string name, Character c, int duration, Mode mode, Sprite s, int bonusId) : base(name, c, duration, mode, s) { this.bonusId = bonusId; }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        if (bonusId == 5) connected.att_e += 50;
        if (bonusId == 6) connected.att_f += 50;
        if (bonusId == 7) connected.att_a += 50;
        if (bonusId == 8) connected.att_w += 50;
        if (bonusId == 9) connected.res_e += 50;
        if (bonusId == 10) connected.res_f += 50;
        if (bonusId == 11) connected.res_a += 50;
        if (bonusId == 12) connected.res_w += 50;
        if (bonusId == 13) {
            connected.att_e += 50;
            connected.att_f += 50;
            connected.att_a += 50;
            connected.att_w += 50;
        }
        if (bonusId == 14) {
            connected.res_e += 50;
            connected.res_f += 50;
            connected.res_a += 50;
            connected.res_w += 50;
        }
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        if (bonusId == 1) connected.incrementPA(1);
        if (bonusId == 2) connected.incrementPM(1);
        if (bonusId == 3) connected.incrementPA(2);
        if (bonusId == 4) connected.incrementPM(2);
        if (bonusId == 15) connected.receiveHeal(100);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        if (bonusId == 5) connected.att_e -= 50;
        if (bonusId == 6) connected.att_f -= 50;
        if (bonusId == 7) connected.att_a -= 50;
        if (bonusId == 8) connected.att_w -= 50;
        if (bonusId == 9) connected.res_e -= 50;
        if (bonusId == 10) connected.res_f -= 50;
        if (bonusId == 11) connected.res_a -= 50;
        if (bonusId == 12) connected.res_w -= 50;
        if (bonusId == 13) {
            connected.att_e -= 50;
            connected.att_f -= 50;
            connected.att_a -= 50;
            connected.att_w -= 50;
        }
        if (bonusId == 14) {
            connected.res_e -= 50;
            connected.res_f -= 50;
            connected.res_a -= 50;
            connected.res_w -= 50;
        }
    }

}
