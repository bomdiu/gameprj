using UnityEngine;
using System.Collections;

public class SceneStartDelay : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Match this to your SceneTransitionManager fade duration (usually 1.0)")]
    public float startDelay = 1.1f; 

    private void Awake()
    {
        // Shut everything down the moment the scene wakes up
        SetGameplayActive(false);
    }

    private IEnumerator Start()
    {
        // Wait for the transition fade to finish
        yield return new WaitForSeconds(startDelay);
        
        // Wake the game up
        SetGameplayActive(true);
        Debug.Log("Gameplay Activated");
    }

    private void SetGameplayActive(bool isActive)
    {
        // 1. Toggle all Enemies
        ChargeEnemyAI[] enemies = Object.FindObjectsByType<ChargeEnemyAI>(FindObjectsSortMode.None);
        foreach (var enemy in enemies) enemy.enabled = isActive;

        // 2. Toggle the Player Components
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Toggle Movement
            if (player.TryGetComponent(out PlayerMovement move)) 
                move.enabled = isActive;

            // Toggle Combat (Matches your Player_Combat name)
            if (player.TryGetComponent(out PlayerCombat combat)) 
                combat.enabled = isActive;

            // Toggle Skills (Optional but recommended)
            if (player.TryGetComponent(out Skill1 s1)) s1.enabled = isActive;
            if (player.TryGetComponent(out Skill2 s2)) s2.enabled = isActive;
        }
    }
}