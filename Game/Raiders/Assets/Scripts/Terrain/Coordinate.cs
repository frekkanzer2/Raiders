using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate {
    public int row;
    public int column;

    public static float VERTICAL_DIFF = 2.8f;
    public static float HORIZONTAL_DIFF = 1.4f;

    public Coordinate(int r, int c) {
        row = r;
        column = c;
    }
    public static Vector2 getPosition(Coordinate c) {
        Vector2 row_based = new Vector2(c.row * VERTICAL_DIFF, c.row * HORIZONTAL_DIFF * -1);
        Vector2 col_based = new Vector2(c.column * VERTICAL_DIFF, c.column * HORIZONTAL_DIFF);
        if (Map.Instance == null) return (row_based + col_based);
        return (row_based + col_based) + Map.Instance.getPositionOfParent();
    }
    public static int getBlockZindex(Coordinate c) {
        return c.row + (c.column * -1) -10000;
    }

    public bool equalsTo(Coordinate c) {
        return row == c.row && column == c.column;
    }
    
    public string display() {
        return "[" + this.row + "," + this.column + "]";
    }

}
