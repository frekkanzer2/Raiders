using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetrificationEvent : ParentEvent
{

    int paToLose = 0;

    public PetrificationEvent(string name, Character c, int duration, Mode mode, Sprite s, int paToLose) : base(name, c, duration, mode, s) { this.paToLose = paToLose; }

    override public void execute() {
        base.execute();
        connected.decrementPA(paToLose);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
