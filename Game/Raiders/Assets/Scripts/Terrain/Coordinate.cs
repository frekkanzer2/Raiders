using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate {
    public int row;
    public int column;
    public Coordinate(int r, int c) {
        row = r;
        column = c;
    }
    public static Vector2 getPosition(Coordinate c) {
        Vector2 row_based = new Vector2(c.row * 2.5f, c.row * -1.5f);
        Vector2 col_based = new Vector2(c.column * 2.5f, c.column * 1.5f);
        return row_based + col_based;
    }
    public static int getBlockZindex(Coordinate c) {
        return c.row + (c.column * -1);
    }

    public bool equalsTo(Coordinate c) {
        return row == c.row && column == c.column;
    }
    
    public string display() {
        return "[" + this.row + "," + this.column + "]";
    }

}
