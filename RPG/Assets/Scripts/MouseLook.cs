using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour {

    public float lookSpeed;

    private const float MAX_ANGLE = 90f;

    Vector2 currentEulerAngles = new Vector2(0, 0);

    void Start() { }

    void Update() {

        // Change our rotation vector based on mouse input
        currentEulerAngles.y += Input.GetAxisRaw("Mouse X") * lookSpeed;
        currentEulerAngles.x -= Input.GetAxisRaw("Mouse Y") * lookSpeed;

        // Clamp the rotation so we can't roll the camera
        currentEulerAngles.x =
                Mathf.Clamp(currentEulerAngles.x, -MAX_ANGLE, MAX_ANGLE);

        // Rotate the parent object in the y-axis
        // (this will also rotate the camera)
        transform.parent.transform.eulerAngles = new Vector3(
            transform.parent.transform.eulerAngles.x,
            currentEulerAngles.y,
            transform.parent.transform.eulerAngles.z
        );

        // Rotate the camera in the x-axis
        // (the parent object stays upright)
        transform.eulerAngles = new Vector3(
            currentEulerAngles.x,
            transform.eulerAngles.y,
            transform.eulerAngles.z
        );
    }

}
