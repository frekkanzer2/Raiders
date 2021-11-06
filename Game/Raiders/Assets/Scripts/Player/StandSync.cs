using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandSync : MonoBehaviour
{

    SpriteRenderer rend;

    // Start is called before the first frame update
    void Start()
    {
        this.rend = GetComponent<SpriteRenderer>();
        rend.sortingOrder = -8000;
    }

    // Update is called once per frame
    void Update()
    {
        GameObject hero = this.transform.parent.transform.gameObject;
        if (hero != null) {
            this.transform.position = hero.transform.position;
        }
    }
}
