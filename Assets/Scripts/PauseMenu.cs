using UnityEngine;
using UnityEngine.UI; // Thêm thư viện UI để xử lý Image
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pausePanel;
    public GameObject settingPanel;

    // Thêm biến để xử lý shader (như hướng dẫn trước)
    public Image backgroundPauseImage; 
    private Material spotlightMat;

    void Start()
    {
        // Tạo bản sao Material để không bị lỗi đổi màu gốc
        if (backgroundPauseImage != null)
        {
            spotlightMat = new Material(backgroundPauseImage.material);
            backgroundPauseImage.material = spotlightMat;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        settingPanel.SetActive(false);
        Time.timeScale = 1f; // Chạy lại game
        GameIsPaused = false;
    }

    void PauseGame()
    {
        // 1. ĐÓNG BĂNG THỜI GIAN NGAY LẬP TỨC (Ưu tiên số 1)
        Time.timeScale = 0f;
        GameIsPaused = true;

        // 2. Hiện UI
        pausePanel.SetActive(true);

        // 3. Xử lý Shader (Đặt trong try-catch để nếu lỗi cũng không sao)
        try 
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && spotlightMat != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(player.transform.position);
                float x = screenPos.x / Screen.width;
                float y = screenPos.y / Screen.height;
                spotlightMat.SetVector("_Center", new Vector4(x, y, 0, 0));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Không tìm thấy Player hoặc lỗi Shader: " + e.Message);
        }
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        pausePanel.SetActive(true);
        settingPanel.SetActive(false);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}