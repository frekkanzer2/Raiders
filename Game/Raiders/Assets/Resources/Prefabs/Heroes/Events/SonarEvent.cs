using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarEvent : ParentEvent
{

    int prev_range = 0;

    public SonarEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        prev_range = connected.spells[0].maxRange;
        connected.spells[0].maxRange = connected.spells[0].maxRange + 4;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.spells[0].maxRange = prev_range;
    }

}
