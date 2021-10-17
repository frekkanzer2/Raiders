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
        // sum of ini
        int first_ini = 0, second_ini = 0;
        for (int i = 0; i < first.Count; i++) {
            first_ini += first[i].GetComponent<Character>().ini;
            second_ini += second[i].GetComponent<Character>().ini;
        }
        turns.Clear();
        if (first_ini == second_ini) {
            int choise = Random.Range(0, 2);
            if (choise == 0) first_ini++;
            else second_ini++;
        }
        if (first_ini > second_ini)
            for (int i = 0; i < first.Count; i++) {
                turns.Add(first[i]);
                turns.Add(second[i]);
            }
        else
            for (int i = 0; i < first.Count; i++) {
                turns.Add(second[i]);
                turns.Add(first[i]);
            }
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
