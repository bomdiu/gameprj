using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Kiểm tra xem có cảnh tiếp theo không
            int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;
            
            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                Debug.Log("Màn chơi tiếp theo hợp lệ. Đang tải...");
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                // Nếu báo lỗi này, nghĩa là bạn chưa kéo đủ các màn vào Build Settings
                Debug.LogError("KHÔNG TÌM THẤY MÀN TIẾP THEO! Hãy kiểm tra Build Settings.");
            }
        }
        else
        {
            Debug.Log("Vật chạm vào không phải Player. Tag của nó là: " + other.tag);
        }
    }
}