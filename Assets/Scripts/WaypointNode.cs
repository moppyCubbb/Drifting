using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    public float minDistanceToReachWaypoint = 5;

    public WaypointNode[] nextWaypointNode;

    public float maximumSpeed = 0;
}
