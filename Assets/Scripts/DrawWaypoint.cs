using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawWaypoint : MonoBehaviour
{
    public GameObject WaypointRootNode;

    WaypointNode[] waypointNodes;

    void Update()
    {
        waypointNodes = GetComponentsInChildren<WaypointNode>();
        foreach (WaypointNode node in waypointNodes)
        {
            DrawConnection(node);
        }
    }

    void DrawConnection(WaypointNode waypointNode)
    {
        foreach (var node in waypointNode.nextWaypointNode)
        {
            Debug.DrawLine(waypointNode.transform.position, node.transform.position, Color.blue);
        }
    }
}
