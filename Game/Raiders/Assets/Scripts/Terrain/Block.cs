using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {
    
    public GameObject linkedObject;
    public Coordinate coordinate;

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
