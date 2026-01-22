using UnityEngine;
using UnityEngine.EventSystems; // Cần thư viện này để bắt sự kiện chuột

public class ButtonAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Cài đặt")]
    public float scaleSize = 1.1f; // Phóng to lên 1.1 lần (110%)
    public float speed = 15f;      // Tốc độ phóng to (càng cao càng nhanh)

    private Vector3 originalScale;
    private Vector3 targetScale;

    void Start()
    {
        // Ghi nhớ kích thước ban đầu của nút
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    // Khi chuột đi vào nút -> Đặt mục tiêu là phóng to
    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = originalScale * scaleSize;
    }

    // Khi chuột rời khỏi nút -> Đặt mục tiêu là về như cũ
    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = originalScale;
    }

    void Update()
    {
        // Vector3.Lerp giúp chuyển đổi giá trị từ A sang B một cách mượt mà theo thời gian
        // Thay vì đổi kích thước cái "bụp", nó sẽ lướt từ từ
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);
    }
}