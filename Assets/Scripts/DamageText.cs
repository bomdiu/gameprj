using UnityEngine;
using UnityEngine.UI; // Cần thiết nếu dùng Text tiêu chuẩn

public class DamageText : MonoBehaviour
{
    // Tốc độ di chuyển lên
    public float moveSpeed = 1f; 
    // Thời gian hiển thị trước khi biến mất
    public float duration = 1f; 
    
    private Text damageText;

    private void Awake()
    {
        damageText = GetComponent<Text>();
    }

    private void Start()
    {
        // Tự hủy đối tượng sau thời gian duration
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        // Di chuyển Text lên trên theo trục Y
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
    }

    // Hàm public để script Enemy_Health có thể gọi và thiết lập giá trị
    public void SetDamageValue(int damageAmount)
    {
        // Sát thương luôn là số âm, nên ta dùng Mathf.Abs để hiển thị số dương
        damageText.text = Mathf.Abs(damageAmount).ToString();
    }
}