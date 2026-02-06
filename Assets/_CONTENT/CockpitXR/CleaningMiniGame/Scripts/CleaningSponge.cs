using UnityEngine;

public class CleaningSponge : MonoBehaviour
{
    [Header("Cleaning Settings")]
    [SerializeField] private float cleanRadius = 0.04f;
    [SerializeField] private float cleanAmountPerSecond = 0.8f;
    [SerializeField] private float minVelocityToClean = 0.05f;
    
    [Header("Contact Detection")]
    [SerializeField] private Transform contactPoint;
    [SerializeField] private float contactCheckDistance = 0.02f;
    [SerializeField] private LayerMask surfaceLayer;
    
    [Header("Feedback")]
    [SerializeField] private AudioSource scrubAudioSource;
    [SerializeField] private AudioClip[] scrubSounds;
    [SerializeField] [Range(0f, 1f)] private float scrubVolume = 0.6f;
    [SerializeField] private float volumeFadeSpeed = 10f;
    [SerializeField] private float minPitch = 1.2f;
    [SerializeField] private float maxPitch = 1.8f;
    [SerializeField] private ParticleSystem cleaningParticles;
    
    [Header("Smoothing")]
    [SerializeField] private float velocitySmoothing = 0.1f;
    [SerializeField] private float cleaningGracePeriod = 0.15f;
    
    private Vector3 lastPosition;
    private Vector3 smoothedVelocity;
    
    private float cleaningGraceTimer;
    private float targetVolume;
    private float targetPitch = 1f;
    
    private bool isInContact;
    private Vector3 closestContactPoint;
    
    private CleanableSurface currentSurface;

    private void Awake()
    {
        if (contactPoint == null)
            contactPoint = transform;
            
        lastPosition = transform.position;
    }

    private void Update()
    {
        // Calculate smoothed velocity
        Vector3 rawVelocity = (transform.position - lastPosition) / Time.deltaTime;
        smoothedVelocity = Vector3.Lerp(smoothedVelocity, rawVelocity, velocitySmoothing);
        lastPosition = transform.position;
        
        // Check for surface contact
        CheckSurfaceContact();
        
        if (isInContact && currentSurface != null)
        {
            float speed = smoothedVelocity.magnitude;
            
            if (speed >= minVelocityToClean)
            {
                PerformCleaning(speed);
            }
        }
        
        // Handle grace timer
        if (cleaningGraceTimer > 0)
        {
            cleaningGraceTimer -= Time.deltaTime;
        }
        else
        {
            targetVolume = 0f;
            if (cleaningParticles != null && cleaningParticles.isPlaying)
            {
                cleaningParticles.Stop();
            }
        }
        
        UpdateAudio();
        UpdateParticles();
    }

    private void CheckSurfaceContact()
    {
        var colliders = Physics.OverlapSphere(contactPoint.position, cleanRadius, surfaceLayer);
        
        foreach (var col in colliders)
        {
            var surface = col.GetComponent<CleanableSurface>();
            if (surface != null)
            {
                currentSurface = surface;
                closestContactPoint = col.ClosestPoint(contactPoint.position);

                isInContact = true;
                return;
            }
        }
        
        isInContact = false;
        currentSurface = null;
    }

    private void PerformCleaning(float speed)
    {
        // Calculate cleaning amount
        float velocityMultiplier = Mathf.Clamp01(speed);
        float cleanAmount = cleanAmountPerSecond * Time.deltaTime;
        
        // Check if position is wet before attempting to clean
        if (currentSurface.IsPositionWet(contactPoint.position))
        {
            bool didClean = currentSurface.ApplySponge(contactPoint.position, cleanRadius, cleanAmount);
            
            if (didClean)
            {
                cleaningParticles.transform.position = closestContactPoint;
                if (!cleaningParticles.isPlaying)
                {
                    cleaningParticles.Play();
                }
            }
        }

        // Reset grace timer and set target volume/pitch
        cleaningGraceTimer = cleaningGracePeriod;
        targetVolume = scrubVolume * Mathf.Lerp(0.5f, 1f, velocityMultiplier);
        targetPitch = Mathf.Lerp(minPitch, maxPitch, velocityMultiplier);
    }

    private void UpdateAudio()
    {
        if (scrubAudioSource == null || scrubSounds == null || scrubSounds.Length == 0) 
            return;
        
        // Smoothly adjust volume toward target
        scrubAudioSource.volume = Mathf.Lerp(
            scrubAudioSource.volume, 
            targetVolume, 
            Time.deltaTime * volumeFadeSpeed
        );
        
        // Smoothly adjust pitch toward target
        scrubAudioSource.pitch = Mathf.Lerp(
            scrubAudioSource.pitch,
            targetPitch,
            Time.deltaTime * volumeFadeSpeed
        );
        
        if (targetVolume > 0.01f && !scrubAudioSource.isPlaying)
        {
            scrubAudioSource.clip = GetRandomScrubSound();
            scrubAudioSource.loop = false;
            scrubAudioSource.volume = 0f;
            scrubAudioSource.pitch = minPitch;
            scrubAudioSource.Play();
        }
        else if (scrubAudioSource.volume < 0.01f && scrubAudioSource.isPlaying)
        {
            scrubAudioSource.Stop();
        }
    }

    private AudioClip GetRandomScrubSound()
    {
        if (scrubSounds == null || scrubSounds.Length == 0)
            return null;
        
        int index = Random.Range(0, scrubSounds.Length);
        return scrubSounds[index];
    }

    private void UpdateParticles()
    {
        if (cleaningParticles == null)
            return;

        if (!isInContact || smoothedVelocity.magnitude < minVelocityToClean)
        {
            if (cleaningParticles.isPlaying)
                cleaningParticles.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform point = contactPoint != null ? contactPoint : transform;
        Gizmos.color = isInContact ? Color.green : Color.red;
        Gizmos.DrawWireSphere(point.position, cleanRadius);
    }
}
