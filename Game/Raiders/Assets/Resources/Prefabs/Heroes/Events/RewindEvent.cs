using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindEvent : ParentEvent
{

    private Coordinate startPosition;

    public RewindEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        startPosition = connected.connectedCell.GetComponent<Block>().coordinate;
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
        Block targetBlock = Map.Instance.getBlock(startPosition);
        connected.connectedCell.GetComponent<Block>().linkedObject = null;
        connected.connectedCell = targetBlock.gameObject;
        targetBlock.linkedObject = connected.gameObject;
        Vector2 newPosition = Coordinate.getPosition(targetBlock.coordinate);
        connected.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
    }

}
