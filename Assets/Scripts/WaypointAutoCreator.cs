using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
public class WaypointAutoCreator : MonoBehaviour
{
    public bool autoUpdate;

    public Transform waypointRoot;

    Stack<GameObject> waypointPool = new Stack<GameObject>();
    Stack<GameObject> waypointInUse = new Stack<GameObject>();

    private void Start()
    {
        
    }

    public void InitAutoCreator()
    {
        waypointPool.Clear();
        waypointInUse.Clear();
        WaypointNode[] waypointNodes = waypointRoot.GetComponentsInChildren<WaypointNode>();
        if (waypointNodes.Length > 0)
        {
            foreach (WaypointNode waypoint in waypointNodes)
            {
                if (waypoint.isActiveAndEnabled)
                {
                    waypointInUse.Push(waypoint.gameObject);
                }
                else
                {
                    waypointPool.Push(waypoint.gameObject);
                }
            }
        }
    }

    public void UpdateWaypointNodes()
    {
        Path path = GetComponent<PathCreator>().path;
        //Vector2[] points = path.CalculateEvenlySpacedPoints(spacing);
        Vector2[] anchorPoints = path.GetAnchorPoints();

        while (waypointInUse.Count > 0)
        {
            Debug.Log("pop in use");
            GameObject inUse = waypointInUse.Pop();
            inUse.GetComponent<WaypointNode>().nextWaypointNode = null;
            inUse.SetActive(false);
            waypointPool.Push(inUse);
        }
        
        WaypointNode previous = null;
        WaypointNode first = null;
        for (int i = 0; i < anchorPoints.Length; i++)
        {
            GameObject current;
            if (waypointPool.Count == 0)
            {
                Debug.Log("instantiate");
                current = new GameObject();
                current.AddComponent<WaypointNode>();

                current.transform.position = anchorPoints[i];
                current.transform.parent = waypointRoot;
            }
            else
            {
                current = waypointPool.Pop();
            }
            current.transform.name = "Waypoint_" + i.ToString();

            WaypointNode currentWaypoint = current.GetComponent<WaypointNode>();
            if (i == 0)
            {
                first = currentWaypoint;
            }
            else
            {
                previous.nextWaypointNode = new WaypointNode[] {
                    currentWaypoint,
                };
            }
            previous = currentWaypoint;

            waypointInUse.Push(current);
            current.SetActive(true);
        }

        if (path.IsClosed)
        {
            previous.nextWaypointNode = new WaypointNode[]
            {
                first,
            };
        }
    }
}
