using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AStarTupleComparer : IComparer<Tuple<List<Block>, float>>
{
    public int Compare(Tuple<List<Block>, float> x, Tuple<List<Block>, float> y) {
        return (int)x.Item2 - (int)y.Item2;
    }

}
