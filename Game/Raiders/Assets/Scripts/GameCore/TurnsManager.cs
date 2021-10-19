using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnsManager : MonoBehaviour
{

    public static bool isGameStarted = false;
    public static Character active;
    public List<Character> turns = new List<Character>();
    public List<Tuple<GameObject, Character, CharacterInfo>> relations = new List<Tuple<GameObject, Character, CharacterInfo>>();

    public void addRelation(GameObject obj, CharacterInfo ci) {
        relations.Add(new Tuple<GameObject, Character, CharacterInfo>(obj, obj.GetComponent<Character>(), ci));
    }

    public void removeRelation(Character c) {
        Tuple<GameObject, Character, CharacterInfo> toDel = null;
        foreach (Tuple<GameObject, Character, CharacterInfo> t in relations) {
            if (t.Item2.name == c.name && t.Item2.team == c.team) {
                toDel = t;
                break;
            }
        }
        if (toDel != null)
            relations.Remove(toDel);
    }

    public Character getCharacterInTurns(string name, int team) {
        foreach(Character c in turns) {
            if (c.name == name && c.team == team) return c;
        }
        return null;
    }

    private bool hasInitialized = false;

    public void initialize() {
        hasInitialized = true;
        List<Character> first = new List<Character>();
        List<Character> second = new List<Character>();
        Debug.Log("init with n chars in total: " + turns.Count);
        foreach (Character c in turns) {
            if (c.team == 1) first.Add(c);
            if (c.team == 2) second.Add(c);
        }
        foreach(Character c in first) {
            Debug.Log(c.name + " with team " + c.team);
        }
        foreach (Character c in second) {
            Debug.Log(c.name + " with team " + c.team);
        }
        Debug.LogWarning("FIRST CHECKPOINT");
        IniComparer ic = new IniComparer();
        first.Sort(ic);
        second.Sort(ic);
        foreach (Character c in first) {
            Debug.Log(c.name + " with team " + c.team);
        }
        foreach (Character c in second) {
            Debug.Log(c.name + " with team " + c.team);
        }
        Debug.LogWarning("SECOND CHECKPOINT");
        // sum of ini
        int first_ini = 0, second_ini = 0;
        for (int i = 0; i < first.Count; i++) {
            first_ini += first[i].GetComponent<Character>().ini;
            second_ini += second[i].GetComponent<Character>().ini;
        }
        turns.Clear();
        if (first_ini == second_ini) {
            int choise = UnityEngine.Random.Range(0, 2);
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
        foreach (Character c in turns) {
            Debug.Log(c.name + " with team " + c.team);
        }
        Debug.LogWarning("THIRD CHECKPOINT");
        Debug.Log(turns.Count);
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
