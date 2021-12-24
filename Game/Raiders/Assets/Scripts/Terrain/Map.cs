using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map {

    private static Map _instance;
    public static Map Instance { get { return _instance; } }


    public class Row {
        public List<Block> list = new List<Block>();
    }

    public List<Row> map;
    public Row pointer;

    public Map() {
        map = new List<Row>();
        if (Map.Instance == null) Map._instance = this;
    }

    public void addBlock(int row, int column, Block block) {
        if (row >= map.Count) {
            Row r = new Row();
            pointer = r;
            map.Add(r);
        }
        pointer.list.Add(block);
    }

    public Block getBlock(Coordinate coordinate) {
        if (coordinate.row < 0 || coordinate.row > map.Count - 1) return null;
        Row get = map[coordinate.row];
        foreach (Block b in get.list) {
            if (b.coordinate.column == coordinate.column)
                return b;
        }
        return null;
    }

    public List<Block> getAllBlocks() {
        List<Block> all = new List<Block>();
        foreach(Row r in map)
            foreach (Block b in r.list)
                all.Add(b);
        return all;
    }

    public void RESET() {
        foreach (Row r in map)
            r.list.Clear();
        map.Clear();
    }

    public void LogBuffer() {
        Debug.LogWarning("Now there are " + getAllBlocks().Count + " blocks in the buffer");
    }

    public void moveAllBlocksOf(float h, float v) {
        List<Block> all = getAllBlocks();
        GameObject toMove = all[0].gameObject.transform.parent.gameObject;
        toMove.transform.position = new Vector3(toMove.transform.position.x - h, toMove.transform.position.y - v);
    }

    public Vector2 getPositionOfParent() {
        List<Block> all = getAllBlocks();
        if (all == null || all.Count == 0) return Vector2.zero;
        GameObject toMove = all[0].gameObject.transform.parent.gameObject;
        if (toMove == null) return Vector2.zero;
        return toMove.transform.position;
    }

}
