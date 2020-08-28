using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private GameObject mainCamera;
    private CharacterController controller;

    private Vector3 velocity;
    private float playerSpeed = 2.0f;
    private float jumpForce = 1.0f;
    private float gravityValue = -9.81f;

    private void Start() {
        mainCamera = transform.Find("Main Camera").gameObject;
        controller = gameObject.GetComponent<CharacterController>();
    }

    void Update() {

        // Rotate based on the camera orientation
        if (mainCamera != null) {
            transform.rotation = Quaternion.AngleAxis(
                    mainCamera.transform.eulerAngles.y, Vector3.up);
        }

        // Reset downward velocity when grounded
        if (controller.isGrounded && velocity.y < 0) {
            velocity.y = 0f;
        }

        // TODO: Apply deceleration (for now we stop every frame)
        velocity.x = 0;
        velocity.z = 0;

        // Apply acceleration based on player input
        velocity.x += transform.forward.x
                * Input.GetAxis("Vertical")
                * playerSpeed
                * Time.deltaTime;
        velocity.z += transform.forward.z
                * Input.GetAxis("Vertical")
                * playerSpeed
                * Time.deltaTime;

        // TODO: Strafe based on player input

        // Apply gravity
        velocity.y += gravityValue * Time.deltaTime;

        // Allow jumping when grounded
        if (Input.GetButtonDown("Jump") && controller.isGrounded) {
            velocity.y += jumpForce;
        }

        // TODO: Cap velocity

        // Move with collision
        controller.Move(velocity);
    }

}