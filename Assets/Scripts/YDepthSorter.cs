using UnityEngine;

public class YDepthSorter : MonoBehaviour
{
    // Khoảng thời gian cập nhật (để tối ưu hóa, không cần cập nhật mỗi frame)
    public float updateInterval = 0.1f; 

    // Hệ số dùng để điều chỉnh Order in Layer. 
    // Nếu Y tăng khi đi xuống, hãy dùng 1.
    // Nếu Y giảm khi đi xuống, hãy dùng -1.
    // Sử dụng giá trị lớn hơn (ví dụ: 100) để đảm bảo độ chính xác.
    public int sortingFactor = 100; 

    private SpriteRenderer spriteRenderer;
    private float nextUpdateTime = 0f;

    void Start()
    {
        // Lấy tham chiếu đến component SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Kiểm tra nếu không tìm thấy SpriteRenderer, script sẽ không hoạt động
        if (spriteRenderer == null)
        {
            Debug.LogError("YDepthSorter yêu cầu component SpriteRenderer trên cùng GameObject!");
            enabled = false;
            return;
        }

        // Thực hiện sắp xếp ngay lập tức khi khởi tạo
        UpdateSortingOrder();
    }

    void Update()
    {
        // Cập nhật Order in Layer theo khoảng thời gian đã định
        if (Time.time >= nextUpdateTime)
        {
            UpdateSortingOrder();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    void UpdateSortingOrder()
    {
        // Lấy vị trí Y của đối tượng
        float yPosition = transform.position.y;

        // Tính toán Order in Layer. 
        // Lấy phần nguyên của Y nhân với hệ số.
        // Giá trị càng lớn thì được vẽ sau (phía trước).
        int newSortingOrder = Mathf.RoundToInt(-yPosition * sortingFactor);

        // Áp dụng giá trị mới
        spriteRenderer.sortingOrder = newSortingOrder;
    }
}