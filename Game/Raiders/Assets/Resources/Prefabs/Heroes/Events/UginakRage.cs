using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UginakRage : ParentEvent
{

    int powerup = 0;
    Sprite backup = null;

    public UginakRage(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void both_firstExecute() {
        base.both_firstExecute();
        backup = connected.gameObject.GetComponent<SpriteRenderer>().sprite;
        connected.att_a += 60;
        connected.att_e += 60;
        connected.att_w += 60;
        connected.att_f += 60;
        connected.res_a += 25;
        connected.res_e += 25;
        connected.res_w += 25;
        connected.res_f += 25;
        connected.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Prefabs/Heroes/Transformation/Miliboowolf");
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.incrementPM(2);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.gameObject.GetComponent<SpriteRenderer>().sprite = backup;
        connected.att_a -= 60;
        connected.att_e -= 60;
        connected.att_w -= 60;
        connected.att_f -= 60;
        connected.res_a -= 25;
        connected.res_e -= 25;
        connected.res_w -= 25;
        connected.res_f -= 25;
    }

}
