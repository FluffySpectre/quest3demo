using UnityEngine;

public class BrightnessFlash : MonoBehaviour
{
    [SerializeField] private float baseValue = 0f;
    [SerializeField] private float flashValue = 1f;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Renderer _renderer;
    private Material _material;
    private float _timer;
    private bool _flashing;

    private static readonly int BrightnessID = Shader.PropertyToID("_Brightness");

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _material = _renderer.materials[0];
        _material.SetFloat(BrightnessID, baseValue);
    }

    public void Flash()
    {
        _timer = 0f;
        _flashing = true;
    }

    private void Update()
    {
        if (!_flashing) return;

        _timer += Time.deltaTime;
        float t = Mathf.Clamp01(_timer / flashDuration);
        float value = Mathf.LerpUnclamped(baseValue, flashValue, flashCurve.Evaluate(t));
        _material.SetFloat(BrightnessID, value);

        if (t >= 1f)
        {
            _flashing = false;
            _material.SetFloat(BrightnessID, baseValue);
        }
    }

    private void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }
}
