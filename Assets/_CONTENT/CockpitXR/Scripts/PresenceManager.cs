using UnityEngine;
using UnityEngine.Events;

public class PresenceManager : MonoBehaviour
{
    public UnityEvent onHeadsetMounted;
    public UnityEvent onHeadsetRemoved;

    public static PresenceManager Instance { get; private set; }

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
        OVRManager.HMDMounted += OnHMDMounted;
        OVRManager.HMDUnmounted += OnHMDUnmounted;
    }

    void OnDisable()
    {
        OVRManager.HMDMounted -= OnHMDMounted;
        OVRManager.HMDUnmounted -= OnHMDUnmounted;
    }

    void OnHMDMounted()
    {
        onHeadsetMounted.Invoke();
    }

    void OnHMDUnmounted()
    {
        onHeadsetRemoved.Invoke();
    }
}
