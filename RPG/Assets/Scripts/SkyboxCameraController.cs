using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class that controls the Skybox Camera.
 *
 * The idea here is that the skybox camera is enclosed in a physical skybox in
 * a hidden area of the scene. It renders the skybox to the screen every frame,
 * and afterwards the main camera renders the actual game world on top.
 *
 * This is achieved using layers and culling masks; the main camera will not
 * render anything on the "Sky" mask, and the skybox camera renders ONLY this
 * layer.
 */
public class SkyboxCameraController : MonoBehaviour {

    public Camera mainCamera;

    void Start() {
        mainCamera = Camera.main;
    }

    void Update() {
        // Mimic the main camera's rotation
        transform.rotation = mainCamera.transform.rotation;
    }

}
