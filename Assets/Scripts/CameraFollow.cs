using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;

    Vector3 Offset;

    private void Start()
    {
        if (Target)
        {
            transform.position = new Vector3(Target.position.x, Target.position.y, transform.position.z);
            Offset = Target.position - transform.position;
        }
    }

    void LateUpdate()
    {
        if (Target)
        {
            transform.position = Target.position - Offset;
        }
    }
}
