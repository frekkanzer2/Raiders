using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {
    
    public GameObject linkedObject;
    public Coordinate coordinate;

    private bool isSpawnable = false;
    private int spawnableTeam = -1;

    [HideInInspector]
    public bool canMoveHere = false;
    [HideInInspector]
    public bool canAttackHere = false;

    public bool canSpawnHero() {
        return isSpawnable;
    }

    public int getSpawnableTeam() {
        return spawnableTeam;
    }

    public void setSpawnable(int team) {
        isSpawnable = true;
        spawnableTeam = team;
        if (team == 1) setSpawnableTeamAlpha();
        else if (team == 2) setSpawnableTeamBeta();
    }

    public void initialize(Sprite s, Coordinate c) {
        setPosition(c);
        setZindex();
        this.gameObject.GetComponent<SpriteRenderer>().sprite = s;
    }

    private void setPosition(Coordinate c) {
        this.coordinate = c;
        this.gameObject.transform.position = Coordinate.getPosition(this.coordinate);
    }

    private void setZindex() {
        this.GetComponent<SpriteRenderer>().sortingOrder = Coordinate.getBlockZindex(coordinate);
    }

    private void setSpawnableTeamAlpha() {
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(0, 106f/255f, 219f/255f, 1);
    }

    private void setSpawnableTeamBeta() {
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(186f/255f, 0, 4f / 255f, 1);
    }

    public void setMovementColor() {
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(175f/255f, 1, 175f/255f, 1);
        canMoveHere = true;
        canAttackHere = false;
    }

    public void setAttackColor() {
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(217f / 255f, 93f/255f, 95f / 255f, 1);
        canAttackHere = true;
        canMoveHere = false;
    }

    public void setCantAttackColor() {
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(100f / 255f, 150f / 255f, 200f / 255f, 1);
        canAttackHere = false;
        canMoveHere = false;
    }

    public void resetColor() {
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        canMoveHere = false;
        canAttackHere = false;
    }

    public bool equalsTo(Block other) {
        return coordinate.equalsTo(other.coordinate);
    }

    public List<Block> getFreeAdjacentBlocks() {
        List<Block> list = new List<Block>();
        Block adj = Map.Instance.getBlock(new Coordinate(this.coordinate.row, this.coordinate.column+1));
        if (adj != null)
            if (adj.linkedObject == null) list.Add(adj);
        adj = Map.Instance.getBlock(new Coordinate(this.coordinate.row, this.coordinate.column-1));
        if (adj != null)
            if (adj.linkedObject == null) list.Add(adj);
        adj = Map.Instance.getBlock(new Coordinate(this.coordinate.row+1, this.coordinate.column));
        if (adj != null)
            if (adj.linkedObject == null) list.Add(adj);
        adj = Map.Instance.getBlock(new Coordinate(this.coordinate.row-1, this.coordinate.column));
        if (adj != null)
            if (adj.linkedObject == null) list.Add(adj);
        return list;
    }

}
