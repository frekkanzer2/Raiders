using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SacrificeEvent : ParentEvent
{

    public Character target;

    public SacrificeEvent(string name, Character c, int duration, Mode mode, Sprite s, Character t) : base(name, c, duration, mode, s) { target = t; }

    override public void execute() {
        base.execute();
        connected.hasActivedSacrifice = true;
        target.connectedSacrifice = connected;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        connected.hasActivedSacrifice = false;
        target.connectedSacrifice = null;
    }

}
