using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SceneMenuManager : MonoBehaviour
{
    public UnityEvent onMenuShown;
    public UnityEvent onMenuHidden;
    public Transform sceneAnchorPivot;
    public float positionOffsetStep = 0.001f;
    public float rotationOffsetStep = 0.5f;

    // VALUE TEXT LABELS
    public TMP_Text posXValueLabel;
    public TMP_Text posYValueLabel;
    public TMP_Text posZValueLabel;
    public TMP_Text rotXValueLabel;
    public TMP_Text rotYValueLabel;
    public TMP_Text rotZValueLabel;

    /// <summary>
    /// The Parent Object of the Menu
    /// </summary>
    [Tooltip("The parent object of the menu")]
    [Header("Place the grabbable parent object here")]
    [SerializeField]
    private GameObject _menuParent;

    /// <summary>
    /// The audio to play when showing the menu panel
    /// </summary>
    [Tooltip("The audio to play when showing the menu panel")]
    [Header("Place the menu open audio here")]
    [SerializeField]
    private AudioSource _showMenuAudio;

    /// <summary>
    /// The audio to play when hiding the menu panel
    /// </summary>
    [Tooltip("The audio to play when hiding the menu panel")]
    [Header("Place the menu hide audio here")]
    [SerializeField]
    private AudioSource _hideMenuAudio;

    /// <summary>
    /// The location the menu should be spawning at
    /// </summary>
    [Tooltip("The location the menu should be spawning at")]
    [Header("The location the menu should be spawning at")]
    [SerializeField]
    private GameObject _spawnPoint;

    protected bool _started = false;

    void Start()
    {
        SaveDefaultOffsets();
        LoadOffsets();
        UpdateValueLabels();
    }

    /// <summary>
    /// Show/hide the menu.
    /// </summary>
    public void ToggleMenu()
    {
        if (_menuParent.activeSelf)
        {
            _hideMenuAudio.Play();
            _menuParent.SetActive(false);

            onMenuHidden.Invoke();
        }
        else
        {
            _showMenuAudio.Play();
            _menuParent.transform.position = _spawnPoint.transform.position;
            _menuParent.transform.rotation = _spawnPoint.transform.rotation;
            _menuParent.SetActive(true);

            onMenuShown.Invoke();
        }
    }

    void SaveDefaultOffsets()
    {
        // Save the default offsets if not already saved
        if (!PlayerPrefs.HasKey("DefaultSceneAnchorPosX"))
        {
            PlayerPrefs.SetFloat("DefaultSceneAnchorPosX", sceneAnchorPivot.localPosition.x);
            PlayerPrefs.SetFloat("DefaultSceneAnchorPosY", sceneAnchorPivot.localPosition.y);
            PlayerPrefs.SetFloat("DefaultSceneAnchorPosZ", sceneAnchorPivot.localPosition.z);
            PlayerPrefs.SetFloat("DefaultSceneAnchorRotX", sceneAnchorPivot.localEulerAngles.x);
            PlayerPrefs.SetFloat("DefaultSceneAnchorRotY", sceneAnchorPivot.localEulerAngles.y);
            PlayerPrefs.SetFloat("DefaultSceneAnchorRotZ", sceneAnchorPivot.localEulerAngles.z);
        }
    }

    void LoadOffsets()
    {
        // Load saved offsets from PlayerPrefs and restore them
        if (!PlayerPrefs.HasKey("SceneAnchorPosX"))
        {
            // No saved data
            return;
        }
        var savedPos = new Vector3(
            PlayerPrefs.GetFloat("SceneAnchorPosX", 0f),
            PlayerPrefs.GetFloat("SceneAnchorPosY", 0f),
            PlayerPrefs.GetFloat("SceneAnchorPosZ", 0f)
        );
        sceneAnchorPivot.localPosition = savedPos;

        var savedRot = new Vector3(
            PlayerPrefs.GetFloat("SceneAnchorRotX", 0f),
            PlayerPrefs.GetFloat("SceneAnchorRotY", 0f),
            PlayerPrefs.GetFloat("SceneAnchorRotZ", 0f)
        );
        sceneAnchorPivot.localEulerAngles = savedRot;
    }

    void SaveOffsets()
    {
        // Save current offsets to PlayerPrefs
        var pos = sceneAnchorPivot.localPosition;
        PlayerPrefs.SetFloat("SceneAnchorPosX", pos.x);
        PlayerPrefs.SetFloat("SceneAnchorPosY", pos.y);
        PlayerPrefs.SetFloat("SceneAnchorPosZ", pos.z);

        var rot = sceneAnchorPivot.localEulerAngles;
        PlayerPrefs.SetFloat("SceneAnchorRotX", rot.x);
        PlayerPrefs.SetFloat("SceneAnchorRotY", rot.y);
        PlayerPrefs.SetFloat("SceneAnchorRotZ", rot.z);
    }

    public void ResetPositionOffset()
    {
        var defaultPos = new Vector3(
            PlayerPrefs.GetFloat("DefaultSceneAnchorPosX", 0f),
            PlayerPrefs.GetFloat("DefaultSceneAnchorPosY", 0f),
            PlayerPrefs.GetFloat("DefaultSceneAnchorPosZ", 0f)
        );
        sceneAnchorPivot.localPosition = defaultPos;

        SaveOffsets();
        UpdateValueLabels();
    }

    public void ResetRotationOffset()
    {
        var defaultRot = new Vector3(
            PlayerPrefs.GetFloat("DefaultSceneAnchorRotX", 0f),
            PlayerPrefs.GetFloat("DefaultSceneAnchorRotY", 0f),
            PlayerPrefs.GetFloat("DefaultSceneAnchorRotZ", 0f)
        );
        sceneAnchorPivot.localEulerAngles = defaultRot;

        SaveOffsets();
        UpdateValueLabels();
    }

    void UpdateValueLabels()
    {
        Vector3 pos = sceneAnchorPivot.localPosition;
        Vector3 rot = sceneAnchorPivot.localEulerAngles;

        posXValueLabel.text = "X: " + pos.x.ToString("F3");
        posYValueLabel.text = "Y: " + pos.y.ToString("F3");
        posZValueLabel.text = "Z: " + pos.z.ToString("F3");

        rotXValueLabel.text = "X: " + rot.x.ToString("F1");
        rotYValueLabel.text = "Y: " + rot.y.ToString("F1");
        rotZValueLabel.text = "Z: " + rot.z.ToString("F1");
    }

    // POSITION
    public void IncrPositionOffsetX()
    {
        Vector3 pos = sceneAnchorPivot.localPosition;
        pos.x += positionOffsetStep;
        sceneAnchorPivot.localPosition = pos;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void DecrPositionOffsetX()
    {
        Vector3 pos = sceneAnchorPivot.localPosition;
        pos.x -= positionOffsetStep;
        sceneAnchorPivot.localPosition = pos;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void IncrPositionOffsetY()
    {
        Vector3 pos = sceneAnchorPivot.localPosition;
        pos.y += positionOffsetStep;
        sceneAnchorPivot.localPosition = pos;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void DecrPositionOffsetY()
    {
        Vector3 pos = sceneAnchorPivot.localPosition;
        pos.y -= positionOffsetStep;
        sceneAnchorPivot.localPosition = pos;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void IncrPositionOffsetZ()
    {
        Vector3 pos = sceneAnchorPivot.localPosition;
        pos.z += positionOffsetStep;
        sceneAnchorPivot.localPosition = pos;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void DecrPositionOffsetZ()
    {
        Vector3 pos = sceneAnchorPivot.localPosition;
        pos.z -= positionOffsetStep;
        sceneAnchorPivot.localPosition = pos;
        UpdateValueLabels();
        SaveOffsets();
    }

    // ROTATION
    public void IncrRotationOffsetX() 
    {
        Vector3 rot = sceneAnchorPivot.localEulerAngles;
        rot.x += rotationOffsetStep;
        sceneAnchorPivot.localEulerAngles = rot;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void DecrRotationOffsetX()
    {
        Vector3 rot = sceneAnchorPivot.localEulerAngles;
        rot.x -= rotationOffsetStep;
        sceneAnchorPivot.localEulerAngles = rot;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void IncrRotationOffsetY()
    {
        Vector3 rot = sceneAnchorPivot.localEulerAngles;
        rot.y += rotationOffsetStep;
        sceneAnchorPivot.localEulerAngles = rot;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void DecrRotationOffsetY()
    {
        Vector3 rot = sceneAnchorPivot.localEulerAngles;
        rot.y -= rotationOffsetStep;
        sceneAnchorPivot.localEulerAngles = rot;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void IncrRotationOffsetZ()
    {
        Vector3 rot = sceneAnchorPivot.localEulerAngles;
        rot.z += rotationOffsetStep;
        sceneAnchorPivot.localEulerAngles = rot;
        UpdateValueLabels();
        SaveOffsets();
    }

    public void DecrRotationOffsetZ()
    {
        Vector3 rot = sceneAnchorPivot.localEulerAngles;
        rot.z -= rotationOffsetStep;
        sceneAnchorPivot.localEulerAngles = rot;
        UpdateValueLabels();
        SaveOffsets();
    }
}
