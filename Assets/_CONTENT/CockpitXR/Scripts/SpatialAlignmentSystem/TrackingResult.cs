using UnityEngine;

public class TrackingResult
{
    public string MarkerID { get; private set; }
    public Pose Pose { get; private set; }

    public TrackingResult(string markerID, Pose pose)
    {
        MarkerID = markerID;
        Pose = pose;
    }
}
