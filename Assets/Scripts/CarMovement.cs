using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    public float maximumSpeed = 10f;
    public float accelerationFactor = 5.0f;
    public float steerFactor = 2f;
    public float driftFactor = 0.95f;

    Rigidbody2D rb;

    float accelerationInput = 0;
    float steerInput = 0;
    float velocityUp = 0;

    float rotationAngle = 0;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rotationAngle = rb.rotation;
    }

    private void FixedUpdate()
    {
        ApplyEngingForce();

        ApplyFriction();

        ApplySteering();
    }

    private void ApplyEngingForce()
    {
        velocityUp = Vector2.Dot(transform.up, rb.velocity);

        if (velocityUp > maximumSpeed && accelerationInput > 0) return;
        if (velocityUp < -maximumSpeed * 0.5f && accelerationInput < 0) return;
        if (rb.velocity.sqrMagnitude > maximumSpeed * maximumSpeed && accelerationInput > 0) return;

        if (accelerationInput == 0)
        {
            rb.drag = Mathf.Lerp(rb.drag, 3.0f, Time.fixedDeltaTime * 3);
        }
        else
        {
            rb.drag = 0;
        }
        Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;
        
        rb.AddForce(engineForceVector, ForceMode2D.Force);
    }

    private void ApplySteering()
    {
        // limit the car ability to turn when moving slowly
        float minSpeedForTurning = rb.velocity.magnitude / 8;
        minSpeedForTurning = Mathf.Clamp01(minSpeedForTurning);

        // update the rotation angle based on input
        rotationAngle -= steerInput * steerFactor * minSpeedForTurning;
        
        // apply steering by rotating the car object
        rb.MoveRotation(rotationAngle);

        
    }

    //Kill orthogonal velocity
    private void ApplyFriction()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(rb.velocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(rb.velocity, transform.right);

        rb.velocity = forwardVelocity + rightVelocity * driftFactor;
    }

    public void SetInputVector(Vector2 inputVector)
    {
        steerInput = inputVector.x;
        accelerationInput = inputVector.y;
    }

    private float GetLateralVelocity()
    {
        return Vector2.Dot(transform.right, rb.velocity);
    }

    public bool IsScreeching(out float lateralVelocity, out bool isBraking)
    {
        lateralVelocity = GetLateralVelocity();
        isBraking = false;

        if (accelerationInput < 0 && velocityUp > 0)
        {
            isBraking = true;
            return true;
        }

        if (Mathf.Abs(GetLateralVelocity()) > 3.0f)
        {
            isBraking = true;
            return true;
        }

        return false;
    }

    public float GetVelocityMagnitude()
    {
        return rb.velocity.magnitude;
    }
}
