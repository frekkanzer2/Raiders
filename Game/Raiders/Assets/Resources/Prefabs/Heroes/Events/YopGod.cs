using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YopGod : ParentEvent
{

    int powerup = 0;
    Sprite backup = null;

    public YopGod(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void both_firstExecute() {
        base.both_firstExecute();
        backup = connected.gameObject.GetComponent<SpriteRenderer>().sprite;
        connected.att_a += 50;
        connected.att_e += 50;
        connected.att_w += 50;
        connected.att_f += 50;
        connected.res_a -= 20;
        connected.res_e -= 20;
        connected.res_w -= 20;
        connected.res_f -= 20;
        connected.gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Prefabs/Heroes/Transformation/Tristepin Yop God");
    }
    
    override public void both_newTurnExecute() {
        base.both_newTurnExecute();
        connected.incrementPA(1);
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        connected.gameObject.GetComponent<SpriteRenderer>().sprite = backup;
        connected.att_a -= 50;
        connected.att_e -= 50;
        connected.att_w -= 50;
        connected.att_f -= 50;
        connected.res_a += 20;
        connected.res_e += 20;
        connected.res_w += 20;
        connected.res_f += 20;
    }

}
