using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadOrTailEvent : ParentEvent
{

    public HeadOrTailEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        if (UnityEngine.Random.Range(1, 3) == 1)
            connected.receiveHeal(40);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
