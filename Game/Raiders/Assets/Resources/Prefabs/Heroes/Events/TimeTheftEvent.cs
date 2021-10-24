using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTheftEvent : ParentEvent
{

    private Character caster;

    public TimeTheftEvent(string name, Character c, int duration, Mode mode, Sprite s, Character launcher) : base(name, c, duration, mode, s) { caster = launcher; }

    override public void execute() {
        base.execute();
        connected.actual_pa -= 2;
        if (UnityEngine.Random.Range(1, 101) <= 80) {
            caster.actual_pa += 1;
        }
        if (UnityEngine.Random.Range(1, 101) <= 25) {
            caster.actual_pa += 1;
        }
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
