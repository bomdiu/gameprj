using UnityEngine;
using UnityEngine.Rendering.Universal; // Needed for Light2D

public class ExplosionEffect : MonoBehaviour
{
    [Tooltip("How fast the area dims down")]
    public float lightFadeSpeed = 5f;
    private Light2D explosionLight;

    public void StartFade()
    {
        explosionLight = GetComponent<Light2D>();
        if (explosionLight != null)
        {
            explosionLight.enabled = true;
            StartCoroutine(FadeRoutine());
        }
    }

    private System.Collections.IEnumerator FadeRoutine()
    {
        // Smoothly reduce intensity to 0
        while (explosionLight.intensity > 0)
        {
            explosionLight.intensity -= Time.deltaTime * lightFadeSpeed;
            yield return null;
        }
        explosionLight.enabled = false;
    }
}