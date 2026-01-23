using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pausePanel;
    public GameObject settingPanel;

    [Header("Shader Settings")]
    public Image backgroundPauseImage; 
    private Material spotlightMat;

    void Start()
    {
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
            // Safety: Don't allow manual unpausing if the Upgrade Menu is open
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.upgradePanel.activeSelf) 
                return;

            if (GameIsPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        // 1. SET THE FLAG (Everything else looks for this)
        GameIsPaused = true;
        
        // 2. STOP THE CLOCK (Exactly like UpgradeManager)
        Time.timeScale = 0f;

        // 3. SHOW THE UI
        pausePanel.SetActive(true);

        // 4. LOCK PLAYER INPUT
        // This triggers the 'return' in your PlayerMovement script
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.playerMovement != null)
        {
            UpgradeManager.Instance.playerMovement.canMove = false;
        }

        UpdateSpotlightPosition();
    }

    public void ResumeGame()
    {
        // Safety: Prevent unpausing if cards are still on screen
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.upgradePanel.activeSelf)
            return;

        GameIsPaused = false;
        
        // 1. RESTART THE CLOCK
        Time.timeScale = 1f;

        // 2. HIDE THE UI
        pausePanel.SetActive(false);
        settingPanel.SetActive(false);

        // 3. UNLOCK PLAYER INPUT
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.playerMovement != null)
        {
            UpgradeManager.Instance.playerMovement.canMove = true;
        }
    }

    private void UpdateSpotlightPosition()
    {
        if (spotlightMat == null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(player.transform.position);
            float x = screenPos.x / Screen.width;
            float y = screenPos.y / Screen.height;
            spotlightMat.SetVector("_Center", new Vector4(x, y, 0, 0));
        }
    }

    // --- BUTTONS ---
    public void OpenSettings() { pausePanel.SetActive(false); settingPanel.SetActive(true); }
    public void CloseSettings() { pausePanel.SetActive(true); settingPanel.SetActive(false); }
    public void LoadMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene(0); }
    public void QuitGame() { Application.Quit(); }
}