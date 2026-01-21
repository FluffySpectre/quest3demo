using UnityEngine;
using Meta.XR.MRUtilityKit;

public class SceneAnchor : MonoBehaviour
{
    [SerializeField] private MRUKTrackable trackable;

    public void Initialize(MRUKTrackable trackable)
    {
        this.trackable = trackable;
    }

    void Update()
    {
        if (trackable == null)
        {
            return;
        }

        transform.position = trackable.transform.position;

        var eulerAngles = transform.rotation.eulerAngles;
        eulerAngles.y = trackable.transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(eulerAngles);
        
        // transform.rotation = trackable.transform.rotation;
    }
}
