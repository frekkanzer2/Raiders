using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporalParadox : ParentEvent
{

    private Coordinate startPosition;

    public TemporalParadox(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        if (connected.summons[0] != null)
            Spell.EXECUTE_TRANSPOSITION(connected, connected.summons[0].connectedCell.GetComponent<Block>());
    }

}
