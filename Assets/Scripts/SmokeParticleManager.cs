using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmokeParticleManager : MonoBehaviour
{
    ParticleSystem smokeParticleSystem;

    float particleEmssionRate = 0;

    CarMovement carMovement;

    ParticleSystem.EmissionModule smokeParticleSystemEmssionModule;
    // Start is called before the first frame update
    void Start()
    {
        carMovement = GetComponentInParent<CarMovement>();
        smokeParticleSystem = GetComponent<ParticleSystem>();
        if (smokeParticleSystem)
        {
            smokeParticleSystemEmssionModule = smokeParticleSystem.emission;
            smokeParticleSystemEmssionModule.rateOverTime = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        particleEmssionRate = Mathf.Lerp(particleEmssionRate, 0, Time.deltaTime * 5);
        smokeParticleSystemEmssionModule.rateOverTime = particleEmssionRate;
    
        if (carMovement.IsScreeching(out float lateralVelocity, out bool isBraking))
        {
            particleEmssionRate = 30;
        }
        else
        {
            particleEmssionRate = Mathf.Abs(lateralVelocity) * 2;
        }
    }
}
