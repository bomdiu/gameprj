using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverCanvas; // Kéo GameOverCanvas vào
    public Image blackScreen;         // Kéo Panel BlackScreen vào
    public CanvasGroup contentGroup;  // Kéo ContentContainer vào

    [Header("Effects")]
    public ParticleSystem deathParticles; // Kéo Particle System vào
    
    // Singleton để gọi dễ dàng từ bất cứ đâu
    public static GameOverManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        // Đảm bảo ban đầu ẩn hết
        gameOverCanvas.SetActive(false); 
    }

    // Hàm này được gọi khi Player hết máu
    public void TriggerDeath(Transform playerTransform)
    {
        StartCoroutine(DeathSequence(playerTransform));
    }

    IEnumerator DeathSequence(Transform player)
    {
        // 1. Kích hoạt Canvas nhưng chưa hiện nội dung
        gameOverCanvas.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 0); // Trong suốt
        contentGroup.alpha = 0;

        // 2. FREEZE GAME (Dừng mọi hoạt động của quái vật/đạn)
        Time.timeScale = 0f;

        // 3. Xử lý Nhân vật "Tan biến"
        // Di chuyển Particle đến vị trí nhân vật
        deathParticles.transform.position = player.position;
        deathParticles.Play(); // Bùm!

        // Ẩn Sprite nhân vật đi (giả vờ là đã tan biến)
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite != null) playerSprite.enabled = false;

        // 4. Màn hình tối dần (Fade to Black)
        // Lưu ý: Dùng WaitForSecondsRealtime vì Time.timeScale đang là 0
        float duration = 1.5f;
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime; // Dùng thời gian thực
            float alpha = Mathf.Lerp(0, 1, timer / duration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // 5. Hiện Pop-up Lựa chọn
        timer = 0;
        duration = 1.0f; // Hiện chậm rãi trong 1 giây
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            contentGroup.alpha = Mathf.Lerp(0, 1, timer / duration);
            yield return null;
        }
        
        // Mở khóa chuột nếu game ẩn chuột
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // --- Các hàm cho Button ---

    public void RetryGame()
    {
        Time.timeScale = 1f; // Trả lại thời gian trước khi load
        SceneManager.LoadScene(1); // Load Scene chơi game (Index 1)
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // Load Main Menu (Index 0)
    }
}