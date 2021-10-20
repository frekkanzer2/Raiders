using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NumbersDisplayer : MonoBehaviour
{

    public GameObject txt;

    public enum Type {
        Damage,
        Heal,
        PA,
        PM
    }
    
    public void init(Type t, int number, Vector2 startingPosition) {
        if (number >= 100) this.gameObject.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        this.gameObject.transform.position = startingPosition;
        Color c = new Color(0, 0, 0, 0);
        if (t == Type.Damage) c = new Color(1, 0, 0, 1);
        else if (t == Type.Heal) c = new Color(234f / 255f, 149f / 255f, 232f / 255f, 1);
        else if (t == Type.PA) c = new Color(1, 130f / 255f, 0, 1);
        else if (t == Type.PM) c = new Color(0, 176f / 255f, 16f / 255f, 1);
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.color = c;
        tmp.text = "" + number;
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
