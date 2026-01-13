using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    private Vector3 originalPos;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        originalPos = transform.localPosition;
    }

    public void TriggerShake(float duration, float magnitude)
    {
        // This ensures the camera handles its own coroutine
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.unscaledDeltaTime; // Works during Hitstop

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}