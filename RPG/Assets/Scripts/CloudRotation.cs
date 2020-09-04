using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudRotation : MonoBehaviour {

    public float rotSpeed;

    void Start() {
    }

    void Update() {
        transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime);
    }

}
