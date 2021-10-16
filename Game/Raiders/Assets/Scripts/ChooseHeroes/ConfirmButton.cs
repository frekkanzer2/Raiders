using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmButton : MonoBehaviour
{

    public Sprite deny;
    public Sprite validate;
    public Sprite confirm;
    public GameObject img;
    private Image cimg;

    [HideInInspector]
    public int state = -1;

    // Start is called before the first frame update
    void Start()
    {
        cimg = img.GetComponent<Image>();
        setCanDeny();
        state = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setCanValidate() {
        cimg.sprite = validate;
    }
    public void setCanConfirm() {
        cimg.sprite = confirm;
    }
    public void setCanDeny() {
        cimg.sprite = deny;
    }

}
