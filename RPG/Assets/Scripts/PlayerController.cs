using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public float maxSpeedXZ;
    public float maxSpeedY;
    public float acceleration;
    public float groundFriction;
    public float airFriction;
    public float jumpForce;

    private GameObject mainCamera;
    private CharacterController controller;

    private Vector3 velocity;

    private void Start() {
        mainCamera = transform.Find("Main Camera").gameObject;
        controller = gameObject.GetComponent<CharacterController>();
    }

    void Update() {
        MatchCameraRotation();
        CalculateVelocity();
        Move();
    }

    private void MatchCameraRotation() {
        // Rotate player model to match the camera rotation
        if (mainCamera != null) {
            transform.rotation = Quaternion.AngleAxis(
                    mainCamera.transform.eulerAngles.y, Vector3.up);
        }
    }

    private void CalculateVelocity() {
        ResetVelocity();
        ApplyFriction();
        Accelerate();
        ApplyGravity();
        HandleJump();
        LimitVelocity();
    }

    private void ResetVelocity() {
        // Reset downward velocity when grounded
        if (controller.isGrounded && velocity.y < 0) {
            velocity.y = 0f;
        }
    }

    private void ApplyFriction() {
        if (!ShouldApplyFriction()) {
            return;
        }

        float deceleration = controller.isGrounded
                ? groundFriction
                : airFriction;

        float scaledDeceleration = deceleration * Time.deltaTime;
        Vector2 velocityXZ = new Vector2(velocity.x, velocity.z);
        velocityXZ = ApplyDeceleration(velocityXZ, scaledDeceleration);
        velocity.x = velocityXZ.x;
        velocity.z = velocityXZ.y;
    }

    private bool ShouldApplyFriction() {
        // Only apply friction if no direction is held
        return Input.GetAxisRaw("Vertical") == 0
                && Input.GetAxisRaw("Horizontal") == 0;
    }

    private Vector2 ApplyDeceleration(Vector2 velocity, float deceleration) {
        if (velocity.magnitude == 0) {
            return velocity;
        }
        float newMagnitude = Mathf.Max(0f, velocity.magnitude - deceleration);
        return velocity.normalized * newMagnitude;
    }

    private void Accelerate() {

        // Apply forward acceleration based on player input
        float scaledForwardAcceleration = Input.GetAxisRaw("Vertical")
                * acceleration
                * Time.deltaTime;
        velocity.x += transform.forward.x * scaledForwardAcceleration;
        velocity.z += transform.forward.z * scaledForwardAcceleration;

        // Apply sideways acceleration based on player input
        float scaledSidewaysAcceleration = Input.GetAxisRaw("Horizontal")
                * acceleration
                * Time.deltaTime;
        Vector3 left = Vector3.Cross(transform.forward, Vector3.up);
        velocity.x -= left.x * scaledSidewaysAcceleration;
        velocity.z -= left.z * scaledSidewaysAcceleration;
    }

    private void ApplyGravity() {
        velocity.y += Physics.gravity.y * Time.deltaTime;
    }

    private void HandleJump() {
        if (Input.GetButtonDown("Jump") && controller.isGrounded) {
            velocity.y += jumpForce;
        }
    }

    private void LimitVelocity() {
        // XZ
        Vector2 velocityXZ = new Vector2(velocity.x, velocity.z);
        velocityXZ = Vector2.ClampMagnitude(velocityXZ, maxSpeedXZ);
        velocity.x = velocityXZ.x;
        velocity.z = velocityXZ.y;

        // Y
        velocity.y = Mathf.Clamp(velocity.y, -maxSpeedY, maxSpeedY);
    }

    private void Move() {
        controller.Move(velocity);
    }

}