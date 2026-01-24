using UnityEngine;
using System.Collections;

public class SceneStartDelay : MonoBehaviour
{
    public float startDelay = 3f;

    private void Awake()
    {
        // Disable all instances of the AI and Player scripts at the very start
        SetGameplayActive(false);
    }

    private IEnumerator Start()
    {
        // Optional: Trigger a "3-2-1 GO" UI here if you have one
        Debug.Log("Game starting in " + startDelay + " seconds...");
        
        yield return new WaitForSeconds(startDelay);

        Debug.Log("GO!");
        SetGameplayActive(true);
    }

    private void SetGameplayActive(bool isActive)
    {
        // Disable/Enable the Charge AI
        ChargeEnemyAI[] enemies = FindObjectsByType<ChargeEnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in enemies) enemy.enabled = isActive;

        // Disable/Enable the Player (assuming you have a script named PlayerController)
        // Change "PlayerController" to whatever your movement script is named
        MonoBehaviour player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<MonoBehaviour>();
        if (player != null) player.enabled = isActive;
    }
}