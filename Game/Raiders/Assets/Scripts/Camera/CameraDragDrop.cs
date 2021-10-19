using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDragDrop : MonoBehaviour {
    
    private Vector3 ResetCamera;
    private Vector3 Origin;
    private Vector3 Diference;
    private bool Drag = false;
    private float previousDistance = 0;

    public static bool canMove = true;

    void Start() {
        ResetCamera = new Vector3(0, 0, -20);
        this.transform.position = ResetCamera;
    }
    void LateUpdate() {

        if (canMove) {
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

            if (Input.touchCount >= 2) {
                Vector2 touch0, touch1;
                touch0 = Input.GetTouch(0).position;
                touch1 = Input.GetTouch(1).position;
                if (previousDistance == 0)
                    previousDistance = Mathf.Abs(Vector2.Distance(touch0, touch1));
                float actualDistance = Vector2.Distance(touch0, touch1);
                if (actualDistance < 0) actualDistance = 0;
                if (previousDistance > actualDistance) {
                    // zooming out
                    if (Camera.main.orthographicSize > 10f)
                        Camera.main.orthographicSize -= 0.05f;
                } else if (previousDistance > actualDistance) {
                    // zooming in
                    if (Camera.main.orthographicSize < 30)
                        Camera.main.orthographicSize += 0.05f;
                }
            } else previousDistance = 0;

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
            //RESET CAMERA TO STARTING POSITION WITH RIGHT CLICK
            if (Input.GetMouseButton(1)) {
                Camera.main.transform.position = ResetCamera;
            }
        }
        
    }

}
