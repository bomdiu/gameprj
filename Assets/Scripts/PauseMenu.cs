using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pausePanel;    // Kéo PausePanel vào đây
    public GameObject settingPanel;  // Kéo SettingPanel vào đây

    void Update()
    {
        // Nhấn Esc để mở/đóng menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    // 1. Chức năng Resume (Chơi tiếp)
    public void ResumeGame()
    {
        pausePanel.SetActive(false);
        settingPanel.SetActive(false); // Đảm bảo bảng setting cũng đóng
        Time.timeScale = 1f;          // Game chạy lại bình thường
        GameIsPaused = false;
    }

    void PauseGame()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;          // Ngừng thời gian game
        GameIsPaused = true;
    }

    // 2. Chức năng Settings (Chuyển bảng)
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

    // 3. Chức năng Main Menu
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;          // RESET thời gian về 1 trước khi đổi cảnh
        SceneManager.LoadScene(0);    // Load Scene Main Menu (Index 0)
    }
}