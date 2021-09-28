using UnityEngine;
using System.Linq;

public class CarAIManager : MonoBehaviour
{
    public enum AIMode { followPlayer, followWaypoints, followMouse };

    public AIMode aiMode;
    public float maximumSpeed = 15;
    public bool canAvoidCards = true;
    public float approachWaypointLineDistance = 20f;

    [Range(0.0f, 1.0f)]
    public float skillLevel = 1.0f;
    
    float currentMaximumSpeed;

    Vector3 targetPosition = Vector3.zero;

    // follow player
    Transform targetTransform = null;

    // follow waypoint
    WaypointNode currentWaypoint = null;
    WaypointNode previousWaypoint = null;
    WaypointNode[] allWaypoints;

    // colliders
    PolygonCollider2D polygonCollider2D;

    Vector2 avoidanceVectorLerped = Vector2.zero;

    // components
    CarMovement carMovement;


    void Start()
    {
        carMovement = GetComponent<CarMovement>();
        allWaypoints = FindObjectsOfType<WaypointNode>();
        polygonCollider2D = GetComponentInChildren<PolygonCollider2D>();
        currentMaximumSpeed = maximumSpeed;

        SetMaxSpeedBasedOnSkillLevel(currentMaximumSpeed);
    }

    void FixedUpdate()
    {
        Vector2 inputVector = Vector2.zero;

        switch (aiMode)
        {
            case AIMode.followPlayer:
                FollowPlayer();
                break;
            case AIMode.followWaypoints:
                FollowWaypoints();
                break;
            case AIMode.followMouse:
                FollowMousePosition();
                break;
        }

        inputVector.x = TurnTowardsTarget();
        inputVector.y = ApplyThrottleOrBrake(inputVector.x);

        carMovement.SetInputVector(inputVector);
    }

    private void FollowMousePosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        targetPosition = worldPosition;
    }

    private void FollowPlayer()
    {
        if (targetTransform == null)
        {
            targetTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        if (targetTransform)
        {
            targetPosition = targetTransform.position;
        }
    }

    private float TurnTowardsTarget()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        Debug.DrawRay(transform.position, vectorToTarget, Color.cyan);
        vectorToTarget.Normalize();

        if (canAvoidCards)
        {
            AvoidCars(vectorToTarget, out vectorToTarget);
        }

        float angleToTarget = Vector2.SignedAngle(transform.up, vectorToTarget);
        angleToTarget *= -1;

        // the car turn as much as possible if the angle is greater than 45 degrees
        // and smooth out so if the angle is small the AI to make smaller turn
        float steerAmount = angleToTarget / 45.0f;
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);
        return steerAmount;
    }

    private float ApplyThrottleOrBrake(float x)
    {
        if (carMovement.GetVelocityMagnitude() > currentMaximumSpeed) return 0;

        // apply throttle forward based on how much the car wnats to turn
        float reduceSpeedDueToCornering = Mathf.Abs(x) / 1.0f;

        // if it's a sharp turn apply less speed forward
        return 1.05f - reduceSpeedDueToCornering * skillLevel;
    }

    private void FollowWaypoints()
    {
        if (currentWaypoint == null)
        {
            currentWaypoint = FindClosestWaypoint();
            previousWaypoint = currentWaypoint;
        }

        if (currentWaypoint)
        {
            targetPosition = currentWaypoint.transform.position;
            // store how close we are to the target
            float distanceToWaypoint = ((Vector2)(targetPosition - transform.position)).magnitude;

            if (distanceToWaypoint > approachWaypointLineDistance)
            {
                Vector3 nearestPointOnTheWaypointLine = FindNearestPointOnLine(previousWaypoint.transform.position, currentWaypoint.transform.position, transform.position);

                float segment = distanceToWaypoint / approachWaypointLineDistance;
                targetPosition = (targetPosition + nearestPointOnTheWaypointLine * segment) / (segment + 1);
            }

            if (distanceToWaypoint <= currentWaypoint.minDistanceToReachWaypoint)
            {
                if (currentWaypoint.maximumSpeed > 0)
                {
                    //currentMaximumSpeed = currentWaypoint.maximumSpeed;
                    SetMaxSpeedBasedOnSkillLevel(currentWaypoint.maximumSpeed);
                }
                else
                {
                    //currentMaximumSpeed = maximumSpeed;
                    SetMaxSpeedBasedOnSkillLevel(maximumSpeed);
                }

                previousWaypoint = currentWaypoint;
                currentWaypoint = currentWaypoint.nextWaypointNode[Random.Range(0, currentWaypoint.nextWaypointNode.Length)];
            }
        }
    }

    private WaypointNode FindClosestWaypoint()
    {
        return allWaypoints
            .OrderBy(x => Vector3.Distance(transform.position, x.transform.position))
            .FirstOrDefault();
    }

    bool IsCarsInTheFront(out Vector3 otherCarPosition, out Vector3 otherCarRightVector)
    {
        polygonCollider2D.enabled = false;

        RaycastHit2D raycastHit2d = Physics2D.CircleCast(transform.position + transform.up * 0.5f, 2.75f, Vector2.up, 12, 1 << LayerMask.NameToLayer("Car"));

        polygonCollider2D.enabled = true;

        if (raycastHit2d.collider != null)
        {
            Debug.DrawRay(transform.position, transform.up * 12, Color.red);

            otherCarPosition = raycastHit2d.collider.transform.position;
            otherCarRightVector = raycastHit2d.collider.transform.right;

            return true;
        }
        else
        {
            Debug.DrawRay(transform.position, transform.up * 12, Color.black);
        }

        otherCarPosition = Vector3.zero;
        otherCarRightVector = Vector3.zero;
        return false;
    }

    private void AvoidCars(Vector2 vectorToTarget, out Vector2 newVectorToTarget)
    {
        if (IsCarsInTheFront(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
        {
            Vector2 avoidanceVector = Vector2.zero;

            avoidanceVector = Vector2.Reflect((otherCarPosition - transform.position).normalized, otherCarRightVector);

            float distanceToTarget = (targetPosition - transform.position).magnitude;
            // how much desired the AI has to drive towards the waypoint vs avoiding other cars
            // as we get closer to the waypoint the desire to reach the waypoint increases
            float driveToTargetInfluence = Mathf.Clamp(6.0f / distanceToTarget, 0.30f, 1.0f);

            // the desire to avoid the car
            float avoidanceInfluence = 1.0f - driveToTargetInfluence;
            avoidanceVectorLerped = Vector2.Lerp(avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * 5);

            // new direction affected by the desire of driving to the target and avoid other cars
            newVectorToTarget = vectorToTarget * driveToTargetInfluence + avoidanceVectorLerped * avoidanceInfluence;
            newVectorToTarget.Normalize();

            Debug.DrawRay(transform.position, avoidanceVector * 10, Color.green);
            Debug.DrawRay(transform.position, newVectorToTarget * 10, Color.yellow);

        }
        else
        {
            newVectorToTarget = vectorToTarget;
        }
    }

    Vector2 FindNearestPointOnLine(Vector2 lineStartPosition, Vector2 lineEndPosition, Vector2 point)
    {
        Vector2 lineHeading = lineEndPosition - lineStartPosition;

        float maxDistance = lineHeading.magnitude;
        lineHeading.Normalize();

        Vector2 lineVectorStartToPoint = point - lineStartPosition;
        // projection of the point on the lineHeading
        float dotProduct = Vector2.Dot(lineVectorStartToPoint, lineHeading);

        dotProduct = Mathf.Clamp(dotProduct, 0f, maxDistance);

        return lineStartPosition + lineHeading * dotProduct;
    }

    private void SetMaxSpeedBasedOnSkillLevel(float newSpeed)
    {
        currentMaximumSpeed = Mathf.Clamp(newSpeed, 0, maximumSpeed);

        float skillBasedMaximumSpeed = Mathf.Clamp(skillLevel, 0.3f, 1.0f);
        currentMaximumSpeed = currentMaximumSpeed * skillBasedMaximumSpeed;
    }
}
