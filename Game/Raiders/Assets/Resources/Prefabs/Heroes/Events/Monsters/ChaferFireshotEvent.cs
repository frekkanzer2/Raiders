using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaferFireshotEvent : ParentEvent
{

    bool isTarget;

    public ChaferFireshotEvent(string name, Character c, int duration, Mode mode, Sprite s, bool isTarget) : base(name, c, duration, mode, s) { this.isTarget = isTarget; }

    override public void execute() {
        base.execute();
        if (this.isTarget)
            connected.att_f -= 30;
        else
            connected.att_f += 30;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        if (this.isTarget)
            connected.att_f += 30;
        else
            connected.att_f -= 30;
    }

}
