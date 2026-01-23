using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    void Start()
    {
        PresenceManager.Instance.onHeadsetMounted.AddListener(OnHeadsetMounted);
        PresenceManager.Instance.onHeadsetRemoved.AddListener(OnHeadsetRemoved);
    }

    void OnDestroy()
    {
        PresenceManager.Instance.onHeadsetMounted.RemoveListener(OnHeadsetMounted);
        PresenceManager.Instance.onHeadsetRemoved.RemoveListener(OnHeadsetRemoved);
    }

    void OnHeadsetMounted()
    {
        // Reset scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnHeadsetRemoved()
    {
        Debug.Log("Headset removed");
    }
}
