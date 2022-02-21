using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KwasmutationEvent : ParentEvent
{

    int random = 0;

    public KwasmutationEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }
    
    override public void execute() {
        base.execute();
        Random.InitState((int)System.DateTime.Now.Ticks);
        random = UnityEngine.Random.Range(1, 5);
        if (random == 1)
            connected.res_a -= 200;
        else if (random == 2)
            connected.res_e -= 200;
        else if (random == 3)
            connected.res_w -= 200;
        else if (random == 4)
            connected.res_f -= 200;
    }

    public override void restoreCharacter() {
        base.restoreCharacter();
        if (random == 1)
            connected.res_a += 200;
        else if (random == 2)
            connected.res_e += 200;
        else if (random == 3)
            connected.res_w += 200;
        else if (random == 4)
            connected.res_f += 200;
    }

}
