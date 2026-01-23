using UnityEngine;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

public class QRCodeManager : MonoBehaviour
{
    [SerializeField] private SceneAnchor sceneAnchor;
    [SerializeField] private MRUK mrukInstance;

    public static QRCodeManager Instance { get; private set; }

    void OnValidate()
    {
        if (!mrukInstance && FindAnyObjectByType<MRUK>() is { } mruk && mruk.gameObject.scene == gameObject.scene)
        {
            mrukInstance = mruk;
        }
    }

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
        //s_instance = this;

        sceneAnchor.gameObject.SetActive(false);

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

        // Disable QR code tracking after a short delay
        Invoke(nameof(DisableQRCodeTracking), 1.5f);
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

    void DisableQRCodeTracking()
    {
        mrukInstance.enabled = false;
    }
}
