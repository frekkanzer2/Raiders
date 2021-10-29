using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumbersDisplayer : MonoBehaviour
{

    public GameObject txt;

    public void init(Color c, int value, Vector2 positionSpawn) {
        if (value >= 100) this.gameObject.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
        this.gameObject.transform.position = new Vector2(positionSpawn.x, positionSpawn.y);
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.color = c;
        tmp.text = "" + value;
        StartCoroutine(vanish(1));
    }

    public void init(Color c, string text, Vector2 positionSpawn) {
        this.gameObject.transform.position = new Vector2(positionSpawn.x, positionSpawn.y);
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.color = c;
        tmp.text = text;
        StartCoroutine(vanish(1));
    }

    public void init(Color c, Sprite i, Vector2 positionSpawn) {
        this.gameObject.transform.position = new Vector2(positionSpawn.x - 0.349997f, positionSpawn.y);
        TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
        tmp.color = c;
        tmp.text = "";
        Image img = this.gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Image>();
        img.sprite = i;
        img.color = new Color(1, 1, 1, 1);
        StartCoroutine(vanish(2));
    }

    IEnumerator vanish(int type) {
        // type = 1 for text; type = 2 for icon

        if (type == 1) {
            TextMeshProUGUI tmp = txt.GetComponent<TextMeshProUGUI>();
            while (tmp.color.a > 0.035f) {
                yield return new WaitForSeconds(0.05f);
                this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 0.05f, this.gameObject.transform.position.z);
                tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - 0.035f);
            }
        } else if (type == 2) {
            Image tmp = this.gameObject.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Image>();
            while (tmp.color.a > 0.035f) {
                yield return new WaitForSeconds(0.05f);
                this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 0.08f, this.gameObject.transform.position.z);
                tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, tmp.color.a - 0.045f);
            }
        }
        Destroy(this.gameObject);
    }

}
