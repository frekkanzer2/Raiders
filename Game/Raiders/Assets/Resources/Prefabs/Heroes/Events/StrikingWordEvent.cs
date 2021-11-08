using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrikingWordEvent : ParentEvent
{

    public StrikingWordEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        connected.receiveHeal(25);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
