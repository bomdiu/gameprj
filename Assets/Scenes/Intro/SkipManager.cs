using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SkipManager : MonoBehaviour
{
    [Header("Cài đặt")]
    public CanvasGroup skipButtonGroup; // Kéo nút Skip (có CanvasGroup) vào đây
    public int gameSceneIndex = 1;      // Index của màn chơi kế tiếp (thường là 1)
    public float blinkSpeed = 2.0f;     // Tốc độ nhấp nháy
    public float minAlpha = 0.2f;       // Độ mờ tối đa (mờ nhất)
    public float maxAlpha = 0.7f;       // Độ rõ tối đa (không nên để 1, để 0.7 cho nó ảo)

    private bool isButtonActive = false; // Kiểm tra xem nút đã hiện chưa

    void Start()
    {
        // Đảm bảo ban đầu ẩn hoàn toàn
        skipButtonGroup.alpha = 0;
        skipButtonGroup.interactable = false;
        skipButtonGroup.blocksRaycasts = false;
    }

    void Update()
    {
        // 1. Nếu nút CHƯA hiện -> Chờ người chơi Click chuột
        if (!isButtonActive)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                ShowSkipButton();
            }
        }
        // 2. Nếu nút ĐÃ hiện -> Làm hiệu ứng nhấp nháy liên tục
        else
        {
            GhostEffect();
        }
    }

    void ShowSkipButton()
    {
        isButtonActive = true;
        skipButtonGroup.interactable = true; // Cho phép bấm
        skipButtonGroup.blocksRaycasts = true;
    }

    void GhostEffect()
    {
        // Dùng hàm PingPong để giá trị chạy lên chạy xuống nhịp nhàng
        // Time.time * blinkSpeed: Tốc độ chạy
        // maxAlpha - minAlpha: Khoảng dao động
        float alpha = minAlpha + Mathf.PingPong(Time.time * blinkSpeed, maxAlpha - minAlpha);
        
        skipButtonGroup.alpha = alpha;
    }

    // Hàm gọi khi bấm nút Skip
    public void SkipToGame()
    {
        // Bạn có thể thêm âm thanh "Click" ở đây
        SceneManager.LoadScene(gameSceneIndex);
    }
}