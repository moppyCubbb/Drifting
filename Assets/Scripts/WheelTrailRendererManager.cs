using UnityEngine;
using MLAPI;

public class WheelTrailRendererManager : NetworkBehaviour
{
    [SerializeField]
    private TrailRenderer[] trailRenderers;

    private CarMovement carMovement;

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
