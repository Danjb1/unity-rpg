using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour {

    public float lookSpeed = 2.5f;

    private const float MAX_ANGLE = 85;

    Vector2 currentEulerAngles = new Vector2(0, 0);

    void Start() { }

    void Update() {

        // Change our rotation vector based on mouse input
        currentEulerAngles.y += Input.GetAxis("Mouse X") * lookSpeed;
        currentEulerAngles.x -= Input.GetAxis("Mouse Y") * lookSpeed;

        // Clamp the rotation so we can't roll the camera
        currentEulerAngles.x =
                Mathf.Clamp(currentEulerAngles.x, -MAX_ANGLE, MAX_ANGLE);

        // Finally, rotate the camera
        transform.eulerAngles = (Vector2) currentEulerAngles;
    }

}
