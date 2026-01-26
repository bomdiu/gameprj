using UnityEngine;
using System.Collections;
using Cinemachine; // BẮT BUỘC PHẢI CÓ DÒNG NÀY

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Cinemachine Setup")]
    // Kéo cái Virtual Camera đang quay Boss vào đây
    public CinemachineVirtualCamera virtualCamera; 
    
    private CinemachineBasicMultiChannelPerlin perlinNoise;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (virtualCamera != null)
        {
            // Lấy component Noise từ Virtual Camera
            perlinNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void Shake(float duration, float magnitude)
    {
        // Nếu tìm thấy noise profile thì mới rung
        if (perlinNoise != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine(duration, magnitude));
        }
        else
        {
            Debug.LogWarning("Chưa setup Noise Profile trong Virtual Camera!");
        }
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        // 1. Bật độ mạnh rung lên
        perlinNoise.m_AmplitudeGain = magnitude;

        // 2. Chờ hết thời gian
        // Dùng unscaledDeltaTime để rung kể cả khi game đang slow motion (Hitstop)
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; 
            yield return null;
        }

        // 3. Tắt rung (trả về 0)
        perlinNoise.m_AmplitudeGain = 0f;
    }
}