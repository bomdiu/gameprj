using UnityEngine;
using System.Collections;

public class ExplosionIndicator : MonoBehaviour
{
    private ExplosionSkillData data;
    private float timer = 0f;
    private bool exploded = false;

    // Components
    private Transform coreTransform;
    private MeshRenderer coreMeshRenderer;
    private LineRenderer lineBorder;
    private PolygonCollider2D col; // Dùng Polygon để khớp hình Elip

    // Số lượng điểm để vẽ hình tròn (càng cao càng mượt)
    private int segments = 60; 

    public void Setup(ExplosionSkillData skillData)
    {
        data = skillData;
        CreateVisuals();
    }

    void CreateVisuals()
    {
        // === BƯỚC 1: TÍNH TOÁN TỌA ĐỘ (Geometry Math) ===
        // Tính danh sách các điểm tạo nên hình Elip/Tròn
        Vector3[] borderPoints = new Vector3[segments];
        Vector2[] colliderPoints = new Vector2[segments]; // Collider cần Vector2

        float angleStep = 360f / segments;
        // Bán kính là 1 nửa của kích thước (Size)
        float radiusX = data.areaSize.x / 2f;
        float radiusY = data.areaSize.y / 2f;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            // Công thức hình Elip
            float x = Mathf.Sin(angle) * radiusX;
            float y = Mathf.Cos(angle) * radiusY;

            borderPoints[i] = new Vector3(x, y, 0);
            colliderPoints[i] = new Vector2(x, y);
        }


        // === BƯỚC 2: TẠO VIỀN (LINE RENDERER) ===
        GameObject borderObj = new GameObject("BorderLine");
        borderObj.transform.SetParent(transform);
        borderObj.transform.localPosition = Vector3.zero;

        lineBorder = borderObj.AddComponent<LineRenderer>();
        lineBorder.useWorldSpace = false;
        lineBorder.loop = true;
        lineBorder.positionCount = segments;
        lineBorder.SetPositions(borderPoints); // Gán các điểm vừa tính
        
        lineBorder.startWidth = data.borderWidth;
        lineBorder.endWidth = data.borderWidth;
        lineBorder.startColor = data.borderColor;
        lineBorder.endColor = data.borderColor;
        lineBorder.material = new Material(Shader.Find("Sprites/Default"));


        // === BƯỚC 3: TẠO LÕI (MESH GENERATION) ===
        GameObject coreObj = new GameObject("CoreMesh");
        coreTransform = coreObj.transform;
        coreTransform.SetParent(transform);
        coreTransform.localPosition = Vector3.zero;
        coreTransform.localScale = Vector3.zero; // Bắt đầu nhỏ xíu

        // Thêm Mesh Filter & Renderer
        MeshFilter meshFilter = coreObj.AddComponent<MeshFilter>();
        coreMeshRenderer = coreObj.AddComponent<MeshRenderer>();
        
        // Tạo Mesh từ các điểm
        meshFilter.mesh = GenerateCircleMesh(borderPoints);
        
        // Tạo Material đơn giản cho Mesh
        Material coreMat = new Material(Shader.Find("Sprites/Default"));
        coreMat.color = data.coreColor;
        coreMeshRenderer.material = coreMat;
        coreMeshRenderer.sortingOrder = -1; // Vẽ dưới viền


        // === BƯỚC 4: TẠO HITBOX ===
        col = gameObject.AddComponent<PolygonCollider2D>();
        col.points = colliderPoints; // Hitbox khớp 100% với hình vẽ
        col.isTrigger = true;
        col.enabled = false; 
    }

    // Hàm phụ trợ: Tạo Mesh hình tròn từ danh sách điểm viền
    Mesh GenerateCircleMesh(Vector3[] perimeterPoints)
    {
        Mesh mesh = new Mesh();
        
        // Vertices: Tâm (0,0) + Các điểm viền
        Vector3[] vertices = new Vector3[perimeterPoints.Length + 1];
        vertices[0] = Vector3.zero; // Tâm
        for (int i = 0; i < perimeterPoints.Length; i++)
        {
            vertices[i + 1] = perimeterPoints[i];
        }

        // Triangles: Nối Tâm với 2 điểm viền liền kề
        int[] triangles = new int[perimeterPoints.Length * 3];
        for (int i = 0; i < perimeterPoints.Length; i++)
        {
            triangles[i * 3] = 0;           // Đỉnh tâm
            triangles[i * 3 + 1] = i + 1;   // Điểm hiện tại
            // Điểm tiếp theo (nếu là điểm cuối thì nối về điểm đầu)
            triangles[i * 3 + 2] = (i + 1 >= perimeterPoints.Length) ? 1 : i + 2; 
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    void Update()
    {
        if (exploded) return;

        timer += Time.deltaTime;

        // --- LÕI TO DẦN ---
        // Scale Mesh từ 0 lên 1 (vì Mesh đã được sinh ra với kích thước chuẩn Max Size)
        float progress = Mathf.Clamp01(timer / data.growthTime);
        if (coreTransform != null)
        {
            coreTransform.localScale = Vector3.one * progress;
        }

        if (timer >= data.growthTime)
        {
            StartCoroutine(ExplodeRoutine());
        }
    }

    IEnumerator ExplodeRoutine()
    {
        exploded = true;

        if (data.explosionVFX != null)
        {
            GameObject vfxInstance = Instantiate(data.explosionVFX, transform.position, Quaternion.identity);
            vfxInstance.transform.localScale = Vector3.one * data.vfxScale;
        }

        if (lineBorder) lineBorder.enabled = false;
        if (coreMeshRenderer) coreMeshRenderer.enabled = false;

        col.enabled = true;
        yield return new WaitForSeconds(0.1f); 
        
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("BOOM! Damage: " + data.damage);
            // collision.GetComponent<PlayerHealth>().TakeDamage(data.damage);
        }
    }
}