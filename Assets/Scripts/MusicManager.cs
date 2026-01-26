using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // Cần thiết để quản lý chuyển cảnh

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Transitions")]
    [SerializeField] private float delayBeforeStart = 1.0f;
    [SerializeField] private float fadeInDuration = 2.0f;
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private float targetVolume = 0.5f;

    [Header("Scene Music List")]
    [Tooltip("Gán nhạc tương ứng với tên Scene")]
    public SceneMusic[] sceneMusicList;

    private AudioSource audioSource;
    private Coroutine transitionCoroutine;

    [System.Serializable]
    public struct SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            
            // Đăng ký sự kiện khi scene được load
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh lỗi bộ nhớ
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Tìm bản nhạc phù hợp với tên Scene vừa load
        AudioClip nextClip = null;
        foreach (var item in sceneMusicList)
        {
            if (item.sceneName == scene.name)
            {
                nextClip = item.musicClip;
                break;
            }
        }

        // Bắt đầu quá trình chuyển đổi nhạc
        if (nextClip != null)
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(TransitionToNewMusic(nextClip));
        }
        else
        {
            // Nếu Scene không có nhạc trong danh sách, tắt nhạc dần
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(FadeOutAndStop());
        }
    }

    private IEnumerator TransitionToNewMusic(AudioClip newClip)
    {
        // 1. Nếu đang phát nhạc khác, Fade Out nó trước
        if (audioSource.isPlaying && audioSource.clip != newClip)
        {
            float startVol = audioSource.volume;
            for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVol, 0, t / fadeOutDuration);
                yield return null;
            }
            audioSource.Stop();
        }

        // 2. Delay trước khi bắt đầu nhạc mới
        audioSource.clip = newClip;
        audioSource.volume = 0;
        yield return new WaitForSeconds(delayBeforeStart);

        // 3. Fade In nhạc mới
        audioSource.Play();
        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0, targetVolume, t / fadeInDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVol = audioSource.volume;
        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVol, 0, t / fadeOutDuration);
            yield return null;
        }
        audioSource.Stop();
    }
}