using System.Collections;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private const float FadeDuration = 0.2f;

    private Light _light;

    void Start()
    {
        _light = GetComponent<Light>();

        TimeController.Instance.OnToggle += Fade;
    }

    private void Fade()
    {
        StartCoroutine(TimeController.Instance.IsMove ? FadeCoroutine(1) : FadeCoroutine(0.7f));
    }

    private IEnumerator FadeCoroutine(float target)
    {
        var origIntensity = _light.intensity;
        var t = 0f;
        while (t <= 1)
        {
            _light.intensity = Mathf.Lerp(origIntensity, target, t);
            yield return new WaitForEndOfFrame();
            t += TimeController.Instance.DeltaTime / FadeDuration;
        }
        _light.intensity = target;
    }
}
