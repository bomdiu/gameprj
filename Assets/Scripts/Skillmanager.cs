using UnityEngine;
using UnityEngine.SceneManagement;

public class SkillUnlockManager : MonoBehaviour
{
    public static SkillUnlockManager Instance;

    [Header("Unlock Status")]
    public bool skill1Unlocked = false; 
    public bool skill2Unlocked = false; 

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckLevelCompletion()
    {
        // Trim() removes any accidental hidden spaces at the end of the name
        string currentScene = SceneManager.GetActiveScene().name.Trim();
        Debug.Log("Checking completion for scene: " + currentScene);

        // Use .Equals with OrdinalIgnoreCase to ignore capital letter mistakes
        if (currentScene.Equals("SampleScene 1", System.StringComparison.OrdinalIgnoreCase))
        {
            skill2Unlocked = true;
            Debug.Log("SUCCESS: Skill 2 Unlocked!");
        }
        else if (currentScene.Equals("SampleScene 2", System.StringComparison.OrdinalIgnoreCase))
        {
            skill1Unlocked = true;
            Debug.Log("SUCCESS: Skill 1 Unlocked!");
        }
        else 
        {
            Debug.LogWarning("No unlock logic found for scene name: " + currentScene);
        }
    }
}