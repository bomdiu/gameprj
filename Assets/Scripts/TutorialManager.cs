using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // Kéo chính cái TutorialPanel vào đây (hoặc để trống, script tự tìm)
    public GameObject contentPanel; 

    void Start()
    {
        // Đảm bảo khi game bắt đầu thì bảng này ẩn đi
        // (Trừ khi bạn muốn nó hiện ngay lập tức thì bỏ dòng này)
        // contentPanel.SetActive(false); 
    }

    // Hàm gọi để MỞ bảng
    public void OpenTutorial()
    {
        contentPanel.SetActive(true);
    }

    // Hàm gọi để ĐÓNG bảng
    public void CloseTutorial()
    {
        contentPanel.SetActive(false);
    }
}