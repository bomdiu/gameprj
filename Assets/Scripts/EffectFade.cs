using UnityEngine;

public class EffectCleanup : MonoBehaviour
{
    // This function will now appear in the Animation Event dropdown
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}