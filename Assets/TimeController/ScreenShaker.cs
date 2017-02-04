using System.Collections;
using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
    private float _shake = 0;
    private float _shakeAmount = 0.2f;
    private float _dampingFactor = 15f;
    private float _scaleAmount = 0.25f;

    private Vector3 _originalPos;
    private float _originalSize;
    
    void Start()
    {
        _originalPos = Camera.main.transform.position;
        _originalSize = Camera.main.orthographicSize;

        TimeController.Instance.OnToggle += Shake;
    }

    private void Shake()
    {
        _shake = 2f;
        StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
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
}