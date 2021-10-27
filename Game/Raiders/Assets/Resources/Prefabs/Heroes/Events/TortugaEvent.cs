using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortugaEvent : ParentEvent
{

    public TortugaEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.receiveShield(80);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.removeShield(80);
    }

}
