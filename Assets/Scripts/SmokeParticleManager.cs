using UnityEngine;

public class SmokeParticleManager : MonoBehaviour
{
    [SerializeField]
    private CarMovement carMovement;
    
    private ParticleSystem smokeParticleSystem;
    private float particleEmssionRate = 0;

    ParticleSystem.EmissionModule smokeParticleSystemEmssionModule;
    // Start is called before the first frame update
    void Start()
    {
        //carMovement = GetComponentInParent<CarMovement>();
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
