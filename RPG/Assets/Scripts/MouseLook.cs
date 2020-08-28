using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    public float lookSpeed = 3;

    Vector2 rotation = new Vector2 (0, 0);

    void Start()
    {

    }

    void Update()
    {
        rotation.y += Input.GetAxis("Mouse X");
        rotation.x += -Input.GetAxis("Mouse Y");
        transform.eulerAngles = (Vector2) rotation * lookSpeed;
    }

}
