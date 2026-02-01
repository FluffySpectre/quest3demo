using UnityEngine;

public class CleaningSponge : MonoBehaviour
{
    [Header("Cleaning Settings")]
    [SerializeField] private float cleanRadius = 0.04f;
    [SerializeField] private float cleanAmountPerSecond = 0.8f;
    [SerializeField] private float minVelocityToClean = 0.05f;
    [SerializeField] private float maxVelocityForBonus = 0.5f;
    
    [Header("Contact Detection")]
    [SerializeField] private Transform contactPoint;
    [SerializeField] private float contactCheckDistance = 0.02f;
    [SerializeField] private LayerMask surfaceLayer;
    
    [Header("Feedback")]
    [SerializeField] private AudioSource scrubAudioSource;
    [SerializeField] private AudioClip scrubSound;
    [SerializeField] [Range(0f, 1f)] private float scrubVolume = 0.6f;
    
    private Vector3 lastPosition;
    private Vector3 velocity;
    private float currentWetness;
    private float currentDirtiness;
    
    public bool isGrabbed;
    private bool isInContact;
    
    private CleanableSurface currentSurface;

    private void Awake()
    {
        if (contactPoint == null)
            contactPoint = transform;
            
        lastPosition = transform.position;
    }

    public void OnGrab()
    {
        isGrabbed = true;
    }

    public void OnRelease()
    {
        isGrabbed = false;
        StopCleaningFeedback();
    }

    private void Update()
    {
        // Calculate velocity
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
        
        if (!isGrabbed)
        {
            return;
        }
        
        // Check for surface contact
        CheckSurfaceContact();
        
        if (isInContact && currentSurface != null)
        {
            float speed = velocity.magnitude;
            
            if (speed >= minVelocityToClean)
            {
                PerformCleaning(speed);
            }
            else
            {
                StopCleaningFeedback();
            }
        }
        else
        {
            StopCleaningFeedback();
        }
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
        float velocityMultiplier = Mathf.Clamp01(speed / maxVelocityForBonus);
        float cleanAmount = cleanAmountPerSecond * Time.deltaTime;
        
        // Check if position is wet before attempting to clean
        if (currentSurface.IsPositionWet(contactPoint.position))
        {
            bool didClean = currentSurface.ApplySponge(contactPoint.position, cleanRadius, cleanAmount);
            
            if (didClean)
            {
                // Absorb wetness and dirt
                currentWetness = Mathf.Clamp01(currentWetness + 0.05f * Time.deltaTime);
                currentDirtiness = Mathf.Clamp01(currentDirtiness + 0.03f * Time.deltaTime);
                
                PlayCleaningFeedback(velocityMultiplier);
            }
            else
            {
                StopCleaningFeedback();
            }
        }
        else
        {
            // Surface not wet at this position
            StopCleaningFeedback();
        }
    }

    private void PlayCleaningFeedback(float intensity)
    {
        // Audio
        if (scrubAudioSource != null && !scrubAudioSource.isPlaying && scrubSound != null)
        {
            scrubAudioSource.clip = scrubSound;
            scrubAudioSource.loop = true;
            scrubAudioSource.volume = scrubVolume * intensity;
            scrubAudioSource.Play();
        }
        else if (scrubAudioSource != null && scrubAudioSource.isPlaying)
        {
            scrubAudioSource.volume = Mathf.Lerp(scrubAudioSource.volume, scrubVolume * intensity, Time.deltaTime * 5f);
        }
    }

    private void StopCleaningFeedback()
    {
        if (scrubAudioSource != null && scrubAudioSource.isPlaying)
        {
            scrubAudioSource.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform point = contactPoint != null ? contactPoint : transform;
        Gizmos.color = isInContact ? Color.green : Color.red;
        Gizmos.DrawWireSphere(point.position, cleanRadius);
    }
}
