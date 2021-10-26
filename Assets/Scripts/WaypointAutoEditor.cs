using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(WaypointAutoCreator))]
public class WaypointAutoEditor : Editor
{
    WaypointAutoCreator creator;
    private void OnSceneGUI()
    {
        if (creator.autoUpdate && Event.current.type == EventType.Repaint)
        {
            creator.UpdateWaypointNodes();
        }

    }

    private void OnEnable()
    {
        creator = (WaypointAutoCreator)target;
        creator.InitAutoCreator();
    }
}
#endif
