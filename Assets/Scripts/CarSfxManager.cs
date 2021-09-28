using UnityEngine;

public class CarSfxManager : MonoBehaviour
{
    public AudioSource screechingAudioSource;
    public AudioSource engineAudioSource;
    public AudioSource hitAudioSource;

    float desiredEnginePitch = 0.5f;
    float screechPitch = 0.5f;
    CarMovement carMovement;

    // Start is called before the first frame update
    void Start()
    {
        carMovement = GetComponent<CarMovement>();
        screechingAudioSource.volume = 0;
        engineAudioSource.volume = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEnginSFX();
        UpdateScreechingSFX();
    }

    private void UpdateEnginSFX()
    {
        float velocityMagitude = carMovement.GetVelocityMagnitude();

        float desiredEngineVolumn = Mathf.Abs(velocityMagitude * 0.05f);

        desiredEngineVolumn = Mathf.Clamp(desiredEngineVolumn, 0.2f, 1.0f);

        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, desiredEngineVolumn, Time.deltaTime * 10);

        desiredEnginePitch = velocityMagitude * 0.2f;
        desiredEnginePitch = Mathf.Clamp(desiredEnginePitch, 0.5f, 2f);
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, desiredEnginePitch, Time.deltaTime * 1.5f);
    }

    private void UpdateScreechingSFX()
    {
        if (carMovement.IsScreeching(out float lateralVelocity, out bool isBraking))
        {
            if (isBraking)
            {
                screechingAudioSource.volume = Mathf.Lerp(screechingAudioSource.volume, 1f, Time.deltaTime * 10);
                screechPitch = Mathf.Lerp(screechPitch, 0.5f, Time.deltaTime * 10);
            }
            else
            {
                screechingAudioSource.volume = Mathf.Abs(lateralVelocity) * 0.05f;
                screechPitch = Mathf.Abs(lateralVelocity) * 0.1f;
            }
        }
        else
        {
            // sfx fades out
            screechingAudioSource.volume = Mathf.Lerp(screechingAudioSource.volume, 0, Time.deltaTime * 10);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float relativeVelocity = collision.relativeVelocity.magnitude;

        float volume = relativeVelocity * 0.1f;

        hitAudioSource.pitch = Random.Range(0.95f, 1.05f);
        hitAudioSource.volume = volume;

        if (!hitAudioSource.isPlaying)
        {
            hitAudioSource.Play();
        }
    }
}
