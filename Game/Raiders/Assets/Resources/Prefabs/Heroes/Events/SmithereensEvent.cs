using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmithereensEvent : ParentEvent
{

    public SmithereensEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.smithereensCounter++;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.smithereensCounter--;
    }

}
