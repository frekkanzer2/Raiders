using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMInjector : MonoBehaviour
{

    public List<GameObject> charsToInject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.J)) {
            Debug.LogWarning("Execute only one time!!!\nDo not execute J key anymore!!!");
            TurnsManager tm = this.GetComponent<TurnsManager>();
            foreach(GameObject go in charsToInject) {
                tm.turns.Add(go.GetComponent<Character>());
            }
            tm.initialize();
        }
    }
}
