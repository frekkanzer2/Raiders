using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMInjector : MonoBehaviour
{

    [HideInInspector]
    public List<GameObject> charsToInject;

    public void InjectIntoTurnsManager() {
        SelectionContainer sc = GetComponent<SelectionContainer>();
        charsToInject = sc.getAll();
        TurnsManager tm = this.GetComponent<TurnsManager>();
        foreach (GameObject go in charsToInject) {
            tm.turns.Add(go.GetComponent<Character>());
        }
        tm.initialize();
    }

}
