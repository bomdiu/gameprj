using UnityEngine;
using UnityEngine.UI;

public class ScrollingUV : MonoBehaviour
{
    public RawImage targetImage;
    
    [Header("Tốc độ trôi")]
    [Tooltip("Tốc độ trôi ngang (âm là sang trái, dương là sang phải)")]
    public float speedX = 0.1f;
    [Tooltip("Tốc độ trôi dọc (âm là xuống dưới, dương là lên trên)")]
    public float speedY = 0.05f;

    private Rect uvRect;

    void Start()
    {
        if (targetImage == null) targetImage = GetComponent<RawImage>();
        uvRect = targetImage.uvRect;
    }

    void Update()
    {
        // Cộng dồn vị trí dựa trên thời gian và tốc độ
        uvRect.x += speedX * Time.deltaTime;
        uvRect.y += speedY * Time.deltaTime;
        
        // Gán ngược lại vào RawImage
        targetImage.uvRect = uvRect;
    }
}