using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TrackCreator))]
public class TrackEditor : Editor
{
    TrackCreator creator;

    private void OnSceneGUI()
    {
        if (creator.autoUpdate && Event.current.type == EventType.Repaint)
        {
            creator.UpdateTrack();
        }

    }

    private void OnEnable()
    {
        creator = (TrackCreator)target;
    }
}
