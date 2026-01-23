using UnityEngine;
using UnityEngine.UI;

public class TextureScroll : MonoBehaviour
{
    public RawImage rawImage;
    public float speed = 10f; // Tốc độ nhảy noise

    void Update()
    {
        // Random tọa độ UV Rect để tạo hiệu ứng nhiễu
        float randomX = Random.Range(0f, 1f);
        float randomY = Random.Range(0f, 1f);
        
        // Chỉ thay đổi sau mỗi khoảng thời gian nhất định (để tạo cảm giác frame-by-frame)
        if (Time.frameCount % 5 == 0) // Cứ 5 frame game thì đổi 1 lần
        {
            rawImage.uvRect = new Rect(randomX, randomY, 1f, 1f);
        }
    }
}