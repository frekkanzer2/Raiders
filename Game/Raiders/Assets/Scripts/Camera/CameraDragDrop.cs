using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraDragDrop : MonoBehaviour {
    
    private Vector3 ResetCamera;
    private Vector3 Origin;
    private Vector3 Diference;
    private bool Drag = false;
    private float previousDistance = 0;

    public static bool canMove = true;

    public GameObject sliderZoom;
    private Slider _sliderZoom;

    void Start() {
        ResetCamera = new Vector3(0, 0, -20);
        this.transform.position = ResetCamera;
        _sliderZoom = sliderZoom.GetComponent<Slider>();
        _sliderZoom.value = 22;
    }
    void LateUpdate() {

        if (canMove) {
            // Only pc keys
            if (Input.GetKey(KeyCode.UpArrow)) // forward
            {
                if (Camera.main.orthographicSize < 30)
                    Camera.main.orthographicSize += 0.05f;
            }
            if (Input.GetKey(KeyCode.DownArrow)) // forward
            {
                if (Camera.main.orthographicSize > 10f)
                    Camera.main.orthographicSize -= 0.05f;
            }

            if (Input.GetMouseButton(0)) {
                Diference = (Camera.main.ScreenToWorldPoint(Input.mousePosition)) - Camera.main.transform.position;
                if (Drag == false) {
                    Drag = true;
                    Origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
            } else {
                Drag = false;
            }
            if (Drag == true) {
                Vector3 temp = Origin - Diference;
                if (temp.x > 50) temp.x = 50;
                else if (temp.x < -50) temp.x = -50;
                if (temp.y > 40) temp.y = 40;
                if (temp.y < -40) temp.y = -40;
                Camera.main.transform.position = temp;
            }
        }
        
    }

    private void Update() {
        Camera.main.orthographicSize = _sliderZoom.value;
    }

}
