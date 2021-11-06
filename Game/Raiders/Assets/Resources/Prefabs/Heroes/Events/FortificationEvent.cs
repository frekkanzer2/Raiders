using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FortificationEvent : ParentEvent
{

    public FortificationEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.receiveShield(40);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
