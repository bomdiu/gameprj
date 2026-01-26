using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Required for the 3-second delay

public class SkillUnlockManager : MonoBehaviour
{
    public static SkillUnlockManager Instance;
    public bool skill1Unlocked = false; 
    public bool skill2Unlocked = false; 

    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // This is called by WaveManager when the upgrade panel closes
    public void CheckLevelCompletion() {
        StartCoroutine(DelayedTriggerRoutine());
    }

    private IEnumerator DelayedTriggerRoutine() {
        yield return new WaitForSeconds(2f);

        // 2. TRIGGER: Show everything at once
        string currentScene = SceneManager.GetActiveScene().name.Trim();

        if (currentScene.Equals("SampleScene 1", System.StringComparison.OrdinalIgnoreCase)) {
            skill2Unlocked = true;
            SkillUIController.Instance.TriggerSkillUnlock("Moonblade", "A burst of lunar energy that bounces between enemies.", 2);
        }
        else if (currentScene.Equals("SampleScene 2", System.StringComparison.OrdinalIgnoreCase)) {
            skill1Unlocked = true;
            SkillUIController.Instance.TriggerSkillUnlock("Fireball", "Fires a blazing shot that explodes on impact.", 1);
        }
    }
}