using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUnlockerEvent : ParentEvent
{

    int powerup = 0;
    Sprite backup = null;

    public PowerUnlockerEvent(string name, Character c, int duration, Mode mode, Sprite s, int version) : base(name, c, duration, mode, s) { powerup = version; }
    
    override public void both_firstExecute() {
        base.both_firstExecute();
        backup = connected.gameObject.GetComponent<SpriteRenderer>().sprite;
        Sprite transformationSprite = null;
        if (powerup == 1) {
            transformationSprite = Resources.Load<Sprite>("Prefabs/Heroes/Transformation/Talpotto");
            connected.res_a += 10;
            connected.res_e += 10;
            connected.res_w += 10;
            connected.res_f += 10;
            connected.att_a += 15;
            connected.att_e += 15;
            connected.att_w += 15;
            connected.att_f += 15;
        } else {
            transformationSprite = Resources.Load<Sprite>("Prefabs/Heroes/Transformation/Talpoken") as Sprite;
            connected.att_a += 50;
            connected.att_e += 50;
            connected.att_w += 50;
            connected.att_f += 50;
        }
        connected.gameObject.GetComponent<SpriteRenderer>().sprite = transformationSprite;
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        if (powerup == 2) {
            connected.incrementPA(2);
            connected.decrementPM(2);
            foreach (Character adj in Spell.ut_getAdjacentHeroes(connected.connectedCell.GetComponent<Block>().coordinate))
                if (!adj.isEnemyOf(connected))
                    adj.incrementPA(1);
        }
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.gameObject.GetComponent<SpriteRenderer>().sprite = backup;
        if (powerup == 1) {
            connected.res_a -= 10;
            connected.res_e -= 10;
            connected.res_w -= 10;
            connected.res_f -= 10;
            connected.att_a -= 15;
            connected.att_e -= 15;
            connected.att_w -= 15;
            connected.att_f -= 15;
        } else {
            connected.att_a -= 50;
            connected.att_e -= 50;
            connected.att_w -= 50;
            connected.att_f -= 50;
        }
    }

}
