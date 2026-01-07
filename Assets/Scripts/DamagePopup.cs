using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    private TMP_Text textMesh;
    private MeshRenderer meshRenderer;
    
    [Header("Cài đặt vị trí & Hiển thị")]
    public float spawnYOffset = -0.5f; 
    public float fontSize = 5f;

    [Header("Animation Settings")]
    public float moveSpeed = 0.4f;      
    public float fadeOutTime = 0.6f;    
    
    private Color textColor;
    private float timer;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        // ÉP BUỘC LAYER TRONG START ĐỂ KHÔNG BỊ INSPECTOR ĐÈ
        if (meshRenderer != null)
        {
            // Phải viết đúng 100% tên bạn đã đặt: DamageText
            meshRenderer.sortingLayerName = "DamageText"; 
            meshRenderer.sortingOrder = 500; 
        }
    }

    public void Setup(int damageAmount, Color color)
    {
        if (textMesh == null) textMesh = GetComponent<TMP_Text>();
        
        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString();
            textColor = color; 
            textMesh.color = textColor;
            textMesh.fontSize = fontSize; 
            textMesh.alignment = TextAlignmentOptions.Center;
        }

        // Ép Z = -5f để gần Camera hơn cái cây
        transform.position = new Vector3(transform.position.x, transform.position.y + spawnYOffset, -5f);
        transform.localScale = Vector3.one;
        timer = fadeOutTime; 
    }

    void Update()
    {
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
        timer -= Time.deltaTime;
        
        if (textMesh != null)
        {
            textColor.a = timer / fadeOutTime; 
            textMesh.color = textColor;
        }

        if (timer <= 0) Destroy(gameObject);
    }
}