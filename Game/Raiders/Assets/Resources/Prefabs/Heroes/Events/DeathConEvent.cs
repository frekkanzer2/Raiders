using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathConEvent : ParentEvent
{

    bool b = false;

    public DeathConEvent(string name, Character c, int duration, Mode mode, Sprite s, bool isCaster) : base(name, c, duration, mode, s) { b = isCaster; }

    override public void execute() {
        base.execute();
        if (b)
        {
            Debug.Log("Before: " + connected.att_a);
            connected.att_a += 10;
            Debug.Log("Executed Death Con");
            Debug.Log("Attack: " + connected.att_a);
        } else
        {
            connected.att_a -= 10;
        }
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        if (b)
        {
            connected.att_a -= 10;
        }
        else
        {
            connected.att_a += 10;
        }
    }

}
