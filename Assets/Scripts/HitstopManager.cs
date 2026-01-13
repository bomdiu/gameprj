using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager Instance;
    private bool _isWaiting = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void HitStop(float duration)
    {
        if (duration <= 0 || _isWaiting) return;
        StartCoroutine(DoHitStop(duration));
    }

    private IEnumerator DoHitStop(float duration)
    {
        _isWaiting = true;

        Time.timeScale = 0f; 

        // PHẢI dùng Realtime vì timeScale đang là 0
        yield return new WaitForSecondsRealtime(duration);

        // ÉP về 1.0 để đảm bảo game luôn chạy lại được
        Time.timeScale = 1.0f; 
        
        _isWaiting = false;
    }
}