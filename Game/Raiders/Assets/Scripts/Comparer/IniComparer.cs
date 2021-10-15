using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IniComparer : IComparer<Character> {
    public int Compare(Character x, Character y) {
        return x.ini - y.ini;
    }
}
