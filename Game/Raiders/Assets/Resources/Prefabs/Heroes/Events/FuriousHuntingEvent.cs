using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuriousHuntingEvent : ParentEvent
{

    Character target;

    public FuriousHuntingEvent(string name, Character c, int duration, Mode mode, Sprite s, Character toFollow) : base(name, c, duration, mode, s) { target = toFollow; }

    override public void execute() {
        base.execute();
        Block targetBlock = target.connectedCell.GetComponent<Block>();
        List<Block> frees = targetBlock.getFreeAdjacentBlocks();
        if (frees.Count == 0) return;
        UnityEngine.Random.InitState((int)System.DateTime.Now.Ticks);
        int indexResult = UnityEngine.Random.Range(0, frees.Count);
        targetBlock = frees[indexResult];
        Spell.EXECUTE_JUMP(connected, targetBlock);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
