using System.Collections;
using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
    public static ScreenShaker Instance;

    private float _shake = 0;
    private float _shakeAmount = 0.2f;
    private float _dampingFactor = 15f;
    private float _scaleAmount = 0.5f;

    private Vector3 _originalPos;
    private float _originalSize;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _originalPos = transform.position;
        _originalSize = Camera.main.orthographicSize;
    }

    IEnumerator UnscaledUpdate()
    {
        while (_shake > 0)
        {
            yield return new WaitForEndOfFrame();

            if (_shake > 0.05f)
            {
                Camera.main.orthographicSize = _originalSize - _shake * _scaleAmount;
                Camera.main.transform.position = _originalPos + Random.insideUnitSphere * _shakeAmount;
                _shake -= TimeController.Instance.DeltaTime * _dampingFactor;
            }
            else
            {
                Camera.main.transform.position = _originalPos;
                _shake = 0.0f;
            }
        }
    }

    public void Shake()
    {
        _shake = 2f;
        StartCoroutine(UnscaledUpdate());
    }
}