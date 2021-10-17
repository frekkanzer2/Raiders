using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {
    
    public GameObject linkedObject;
    public Coordinate coordinate;

    private bool isSpawnable = false;
    private int spawnableTeam = -1;

    public bool canSpawnHero() {
        return isSpawnable;
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
    }

    public void resetColor() {
        this.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
    }

    public bool equalsTo(Block other) {
        return coordinate.equalsTo(other.coordinate);
    }

}
