using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelTrailRendererManager : MonoBehaviour
{
    public TrailRenderer[] trailRenderers;
    CarMovement carMovement;

    // Start is called before the first frame update
    void Start()
    {
        carMovement = GetComponent<CarMovement>();

        foreach (var renderer in trailRenderers)
        {
            renderer.emitting = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (carMovement.IsScreeching(out float lateralVelocity, out bool isBraking))
        {
            foreach (var renderer in trailRenderers)
            {
                renderer.emitting = true;
            }
        }
        else
        {
            foreach (var renderer in trailRenderers)
            {
                renderer.emitting = false;
            }
        }
    }
}
