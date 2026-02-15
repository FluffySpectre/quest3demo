using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class MRUKMarkerTracker : MarkerTracker
{
    [SerializeField] private MRUK mrukInstance;

    private readonly Dictionary<string, MRUKTrackable> _trackedMarkers = new();

    void OnValidate()
    {
        if (!mrukInstance && FindAnyObjectByType<MRUK>() is { } mruk
            && mruk.gameObject.scene == gameObject.scene)
        {
            mrukInstance = mruk;
        }
    }

    public override void StartDetection()
    {
        if (!mrukInstance)
        {
            Debug.LogError($"[{nameof(MRUKMarkerTracker)}] MRUK instance not assigned!", this);
            return;
        }

        mrukInstance.enabled = true;
        mrukInstance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
        mrukInstance.SceneSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
    }

    public override void StopDetection()
    {
        if (!mrukInstance) return;

        mrukInstance.SceneSettings.TrackableAdded.RemoveListener(OnTrackableAdded);
        mrukInstance.SceneSettings.TrackableRemoved.RemoveListener(OnTrackableRemoved);
        mrukInstance.enabled = false;
    }

    void Update()
    {
        foreach (var kvp in _trackedMarkers)
        {
            var trackable = kvp.Value;
            if (trackable == null) continue;

            var pose = new Pose(trackable.transform.position, trackable.transform.rotation);
            RaiseMarkerPoseUpdated(new TrackingResult(kvp.Key, pose));
        }
    }

    private void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode) return;

        var payload = trackable.MarkerPayloadString;
        _trackedMarkers[payload] = trackable;

        var pose = new Pose(trackable.transform.position, trackable.transform.rotation);
        RaiseMarkerDetected(new TrackingResult(payload, pose));
    }

    private void OnTrackableRemoved(MRUKTrackable trackable)
    {
        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode) return;

        var payload = trackable.MarkerPayloadString;
        _trackedMarkers.Remove(payload);

        RaiseMarkerLost(payload);

        Destroy(trackable.gameObject);
    }

    void OnDisable()
    {
        _trackedMarkers.Clear();
    }
}
