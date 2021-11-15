using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Prototyping;

public class CarMovement : NetworkBehaviour
{
    public float maximumSpeed = 10f;
    public float accelerationFactor = 5.0f;
    public float steerFactor = 2f;
    public float driftFactor = 0.95f;

    private Rigidbody2D rb;
    private NetworkTransform networkTransform;

    private float accelerationInput = 0;
    private float steerInput = 0;
    private float velocityUp = 0;

    private float rotationAngle = 0;

    public NetworkVariableVector2 Velocity = new NetworkVariableVector2(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.OwnerOnly,
        ReadPermission = NetworkVariablePermission.Everyone,
    });

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rotationAngle = rb.rotation;
        networkTransform = GetComponent<NetworkTransform>();
        if (NetworkManager.Singleton == null)
        {
            networkTransform.enabled = false;
        }
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
        if (NetworkManager.Singleton)
        {
            if (IsOwner)
            {
                Velocity.Value = rb.velocity;
                return Vector2.Dot(transform.right, rb.velocity);
            }
            else
            {
                return Vector2.Dot(networkTransform.transform.right, Velocity.Value);
            }
        }
        else
        {
            return Vector2.Dot(transform.right, rb.velocity);
        }
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
