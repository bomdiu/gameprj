using UnityEngine;

public class Player_SweepSkill : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int energyCost = 50;

    [Header("Recoil Settings")]
    public float recoilForce = 6f;       // độ mạnh đẩy lùi
    public float recoilDuration = 0.1f;  // thời gian đẩy
    [HideInInspector] public bool isRecoiling;

    private Player_Energy energy;
    private Camera mainCam;
    private Rigidbody2D rb;
    private float recoilTimer;
    private Vector2 recoilDir;

    void Start()
    {
        energy = GetComponent<Player_Energy>();
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Chuột phải
        if (Input.GetMouseButtonDown(1))
        {
            CastSkill();
        }
    }

    void FixedUpdate()
    {
        if (isRecoiling)
        {
            rb.velocity = recoilDir * recoilForce;

            recoilTimer -= Time.fixedDeltaTime;
            if (recoilTimer <= 0)
            {
                isRecoiling = false;
                rb.velocity = Vector2.zero;
            }
        }
    }

    void CastSkill()
    {
        Debug.Log("CAST SKILL");

        if (!energy.UseEnergy(energyCost)) return;

        // ===== Hướng chuột =====
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        Vector2 shootDir = (mouseWorldPos - firePoint.position).normalized;

        // ===== Spawn đạn =====
        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.identity
        );
        bullet.GetComponent<Bullet>().Init(shootDir);

        // ===== Recoil ngược hướng bắn =====
        recoilDir = -shootDir;
        recoilTimer = recoilDuration;
        isRecoiling = true;
    }
}
