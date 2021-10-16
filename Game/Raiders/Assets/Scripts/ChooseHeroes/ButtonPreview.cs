using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPreview : MonoBehaviour
{

    public GameObject charImage;
    public GameObject statusImage;

    public Sprite noChar;
    public Sprite empty;
    public Sprite idk;

    [HideInInspector]
    public bool isSet = false;

    // Start is called before the first frame update
    void Start()
    {
        charImage.GetComponent<Image>().sprite = noChar;
        statusImage.GetComponent<Image>().sprite = idk;
    }

    public void setCharacter(Sprite input) {
        charImage.GetComponent<Image>().sprite = input;
        statusImage.GetComponent<Image>().sprite = empty;
        isSet = true;
    }

    public void removeCharacter() {
        charImage.GetComponent<Image>().sprite = noChar;
        statusImage.GetComponent<Image>().sprite = idk;
        isSet = false;
    }

}
