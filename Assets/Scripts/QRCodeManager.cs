using UnityEngine;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

public class QRCodeManager : MonoBehaviour
{
    [SerializeField] private SceneAnchor sceneAnchor;
    [SerializeField] private MRUK mrukInstance;

    void OnValidate()
    {
        if (!mrukInstance && FindAnyObjectByType<MRUK>() is { } mruk && mruk.gameObject.scene == gameObject.scene)
        {
            mrukInstance = mruk;
        }
    }

    void OnEnable()
    {
        //s_instance = this;

        if (!mrukInstance)
        {
            Debug.LogError($"{nameof(QRCodeManager)} requires an MRUK object in the scene!");
            return;
        }

        mrukInstance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
        mrukInstance.SceneSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
    }

    public void OnTrackableAdded(MRUKTrackable trackable)
    {
        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
        {
            return;
        }
        
        //var payload = trackable.MarkerPayloadString;
        //if (payload == "Device1")
        //{
        //    var instance1 = Instantiate(qrCodePrefab, trackable.transform);
        //}

        sceneAnchor.gameObject.SetActive(true);
        sceneAnchor.Initialize(trackable);
    }

    public void OnTrackableRemoved(MRUKTrackable trackable)
    {
        if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
        {
            return;
        }

        Debug.Log($"QRCode removed");

        Destroy(trackable.gameObject);
        sceneAnchor.gameObject.SetActive(false);
    }

}
