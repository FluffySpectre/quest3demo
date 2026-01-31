using UnityEngine;

public class SprayBottle : MonoBehaviour
{
    [SerializeField] private Transform sprayOrigin;
    [SerializeField] private float sprayRange = 0.5f;
    [SerializeField] private float sprayRadius = 0.03f;
    [SerializeField] private float sprayAmount = 0.3f;
    [SerializeField] private float sprayRate = 10f; // sprays per second
    
    private float lastSprayTime;
    public bool isSpraying;
    
    private void Awake()
    {  
        if (sprayOrigin == null)
            sprayOrigin = transform;
    }

    private void Update()
    {

        // isSpraying = true;
        

        if (isSpraying && Time.time - lastSprayTime >= 1f / sprayRate)
        {
            PerformSpray();
            lastSprayTime = Time.time;
        }
    }

    public void PerformSpray()
    {
        // Raycast from spray origin
        if (Physics.Raycast(sprayOrigin.position, sprayOrigin.forward, out RaycastHit hit, sprayRange))
        {
            var cleanableSurface = hit.collider.GetComponentInParent<CleanableSurface>();
            
            if (cleanableSurface != null)
            {
                cleanableSurface.ApplySpray(hit.point, sprayRadius, sprayAmount * Time.deltaTime * sprayRate);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (sprayOrigin == null) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(sprayOrigin.position, sprayOrigin.forward * sprayRange);
        Gizmos.DrawWireSphere(sprayOrigin.position + sprayOrigin.forward * sprayRange, sprayRadius);
    }
}
