using UnityEngine;

public class MarkerManager : MonoBehaviour
{
    [SerializeField] private MarkerTracker tracker;
    [SerializeField] private GameObject sceneAnchor;
    [SerializeField] private string[] validMarkerIDs;
    [SerializeField] private bool stopAfterFirstDetection = true;
    [SerializeField] private float stopDelay = 1.5f;

    public static MarkerManager Instance { get; private set; }

    private string _activeMarkerId;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnEnable()
    {
        if (sceneAnchor != null)
        {
            sceneAnchor.SetActive(false);
        }

        if (tracker == null)
        {
            Debug.LogError($"[{nameof(MarkerManager)}] No MarkerTracker assigned!", this);
            return;
        }

        tracker.MarkerDetected += OnMarkerDetected;
        tracker.MarkerPoseUpdated += OnMarkerPoseUpdated;
        tracker.MarkerLost += OnMarkerLost;

        tracker.StartDetection();
    }

    void OnDisable()
    {
        if (tracker == null) return;

        tracker.StopDetection();

        tracker.MarkerDetected -= OnMarkerDetected;
        tracker.MarkerPoseUpdated -= OnMarkerPoseUpdated;
        tracker.MarkerLost -= OnMarkerLost;
    }

    private void OnMarkerDetected(TrackingResult result)
    {
        string id = result.MarkerID;
        Pose pose = result.Pose;

        if (!IsMarkerIDValid(id))
        {
            Debug.Log($"[{nameof(MarkerManager)}] Ignoring marker with invalid ID: {id}");
            return;
        }

        Debug.Log($"[{nameof(MarkerManager)}] Marker detected: {id}");

        _activeMarkerId = id;
        sceneAnchor.SetActive(true);
        sceneAnchor.transform.SetPositionAndRotation(pose.position, pose.rotation);

        if (stopAfterFirstDetection)
        {
            Invoke(nameof(StopDetector), stopDelay);
        }
    }

    private void OnMarkerPoseUpdated(TrackingResult result)
    {
        string id = result.MarkerID;
        Pose pose = result.Pose;

        if (id != _activeMarkerId) return;

        sceneAnchor.transform.SetPositionAndRotation(pose.position, pose.rotation);
    }

    private void OnMarkerLost(string id)
    {
        if (id != _activeMarkerId) return;

        Debug.Log($"[{nameof(MarkerManager)}] Marker lost: {id}");

        _activeMarkerId = null;
        sceneAnchor.SetActive(false);
    }

    private void StopDetector()
    {
        tracker.StopDetection();
    }

    private bool IsMarkerIDValid(string payload)
    {
        if (validMarkerIDs == null || validMarkerIDs.Length == 0) return true;

        foreach (var id in validMarkerIDs)
        {
            if (payload == id) return true;
        }

        return false;
    }
}
