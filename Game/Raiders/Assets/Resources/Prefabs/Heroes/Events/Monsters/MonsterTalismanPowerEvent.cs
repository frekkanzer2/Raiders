using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterTalismanPowerEvent : ParentEvent
{

    public MonsterTalismanPowerEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.res_a -= 100;
        connected.res_e -= 100;
        connected.res_w -= 100;
        connected.res_f -= 100;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.res_a += 100;
        connected.res_e += 100;
        connected.res_w += 100;
        connected.res_f += 100;
    }

}
