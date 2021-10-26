using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumbersDisplayer : MonoBehaviour
{

    public GameObject txt;

    public void init(Color c, int value, Vector2 positionSpawn) {
        if (value >= 100) this.gameObject.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        this.gameObject.transform.position = positionSpawn;
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.color = c;
        tmp.text = "" + value;
        StartCoroutine(vanish());
    }

    public void init(Color c, string text, Vector2 positionSpawn) {
        this.gameObject.transform.position = positionSpawn;
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.color = c;
        tmp.text = text;
        StartCoroutine(vanish());
    }

    IEnumerator vanish() {
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        while (tmp.color.a > 0.035f) {
            yield return new WaitForSeconds(0.05f);
            this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 0.05f, this.gameObject.transform.position.z);
            tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - 0.035f);
        }
        Destroy(this.gameObject);
    }

}
