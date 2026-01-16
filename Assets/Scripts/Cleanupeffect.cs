using UnityEngine;

public class CleanUpEffect : MonoBehaviour 
{
    public float delay = 0.55f; // Chỉnh thấp hơn tổng thời gian nổ một chút

    void Start() 
    {
        // Ép đối tượng biến mất hoàn toàn trước khi Unity kịp hiện shuriken
        Destroy(gameObject, delay); 
    }
}