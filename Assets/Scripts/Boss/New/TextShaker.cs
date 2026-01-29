using UnityEngine;
using TMPro;

public class TextShaker : MonoBehaviour
{
    public float angleMultiplier = 1.0f; // Độ rung xoay
    public float speedMultiplier = 1.0f; // Tốc độ rung
    public float curveScale = 1.0f;      // Độ méo mó

    private TMP_Text textComponent;
    private bool hasTextChanged;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        // Đăng ký sự kiện để biết khi nào text thay đổi thì cập nhật lại mesh
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
        // Khi tắt script, reset chữ về bình thường ngay lập tức
        if(textComponent != null) textComponent.ForceMeshUpdate(); 
    }

    void ON_TEXT_CHANGED(Object obj)
    {
        if (obj == textComponent) hasTextChanged = true;
    }

    void Update()
    {
        // Chỉ chạy hiệu ứng khi script này được Enable (Bật)
        if (textComponent == null) return;

        // Cập nhật lại mesh text để chuẩn bị biến đổi
        textComponent.ForceMeshUpdate();
        
        var textInfo = textComponent.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

            // Lấy 4 đỉnh của 1 ký tự
            // Logic rung: Dùng hàm Sin/Cos theo thời gian để tạo dao động
            for (int j = 0; j < 4; j++)
            {
                var orig = verts[charInfo.vertexIndex + j];
                
                // Công thức tạo độ rung ngẫu nhiên (Jitter)
                float jitterX = Mathf.Sin(Time.time * speedMultiplier + orig.y * 0.01f) * curveScale;
                float jitterY = Mathf.Cos(Time.time * speedMultiplier + orig.x * 0.01f) * curveScale;

                verts[charInfo.vertexIndex + j] = orig + new Vector3(jitterX, jitterY, 0);
            }
        }

        // Đẩy dữ liệu vertex mới lên màn hình
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            textComponent.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}