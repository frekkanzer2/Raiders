using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnsManager : MonoBehaviour
{

    public static Character active;
    public List<Character> turns = new List<Character>();

    private bool hasInitialized = false;

    public void initialize() {
        hasInitialized = true;
        List<Character> first = new List<Character>();
        List<Character> second = new List<Character>();
        foreach (Character c in turns) {
            if (c.team == 1) first.Add(c);
            if (c.team == 2) second.Add(c);
        }
        IniComparer ic = new IniComparer();
        first.Sort(ic);
        second.Sort(ic);
        turns.Clear();
        for (int i = 0; i < first.Count; i++) {
            turns.Add(first[i]);
            turns.Add(second[i]);
        }
        active = turns[0];
        Debug.Log("Turn of " + active.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && hasInitialized) {
            // New turn
            Character endTurnCh = turns[0];
            turns.Remove(endTurnCh);
            endTurnCh.turnPassed();
            turns.Add(endTurnCh);
            Character newTurnCh = turns[0];
            newTurnCh.newTurn();
            active = newTurnCh;
            Debug.Log("Turn of " + active.name);
        }
    }
}
