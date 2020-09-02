using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHandler : MonoBehaviour {

    void Start() { }

    void Update() {
        if (Input.GetButton("Cancel")) {
            Application.Quit();
        }
    }

}
