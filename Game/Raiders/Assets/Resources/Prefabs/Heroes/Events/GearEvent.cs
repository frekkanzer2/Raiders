using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearEvent : ParentEvent
{

    public GearEvent(string name, Character c, int duration, Mode mode, Sprite s) : base(name, c, duration, mode, s) { }

    override public void execute() {
        base.execute();
        List<Block> listOfBlocks = Map.Instance.getAllBlocks();
        Block toTeleport = connected.connectedCell.GetComponent<Block>();
        while (toTeleport.linkedObject != null) {
            toTeleport = listOfBlocks[UnityEngine.Random.Range(0, listOfBlocks.Count)];
        }
        connected.connectedCell.GetComponent<Block>().linkedObject = null;
        connected.connectedCell = toTeleport.gameObject;
        toTeleport.linkedObject = connected.gameObject;
        Vector2 newPosition = Coordinate.getPosition(toTeleport.coordinate);
        connected.transform.position = new Vector3(newPosition.x, newPosition.y, -20);
    }

    override public void restoreCharacter() {
        base.restoreCharacter();
    }

}
