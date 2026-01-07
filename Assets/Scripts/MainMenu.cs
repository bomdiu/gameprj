using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Kéo các Panel tương ứng vào đây trong Inspector
    public GameObject mainPanel;
    public GameObject settingPanel;

    // 1. Nút Play
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // 2. Nút Settings (Mở bảng cài đặt)
    public void OpenSettings()
    {
        mainPanel.SetActive(false);    // Ẩn menu chính
        settingPanel.SetActive(true);   // Hiện bảng cài đặt
    }

    // Nút Back (Quay lại menu chính)
    public void CloseSettings()
    {
        mainPanel.SetActive(true);
        settingPanel.SetActive(false);
    }

    // 3. Nút Quit
    public void QuitGame()
    {
        Debug.Log("Game đã thoát!");
        Application.Quit();
    }
}