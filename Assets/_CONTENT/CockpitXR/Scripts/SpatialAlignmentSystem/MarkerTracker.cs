using System;
using UnityEngine;

public abstract class MarkerTracker : MonoBehaviour
{
    public event Action<TrackingResult> MarkerDetected;
    public event Action<TrackingResult> MarkerPoseUpdated;
    public event Action<string> MarkerLost;

    protected void RaiseMarkerDetected(TrackingResult result) => MarkerDetected?.Invoke(result);
    protected void RaiseMarkerPoseUpdated(TrackingResult result) => MarkerPoseUpdated?.Invoke(result);
    protected void RaiseMarkerLost(string id) => MarkerLost?.Invoke(id);

    public virtual void StartDetection() { }
    public virtual void StopDetection() { }
}
