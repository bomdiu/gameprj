using UnityEngine;

public class AnimationBridge : MonoBehaviour
{
    private PlayerCombat combat;

    void Start()
    {
        // Finds the script on the parent 'Player' object
        combat = GetComponentInParent<PlayerCombat>();
    }

    // These match your Animation Event names exactly
    public void TriggerHitbox() => combat?.TriggerHitbox();
    
    public void EndAttackMove() => combat?.EndAttackMove();

    public void StartAttackMove() => combat?.StartAttackMove();

    // Fixes the "No Receiver" error for DisableAllHitboxes
    public void DisableAllHitboxes() => combat?.DisableAllHitboxes();
}