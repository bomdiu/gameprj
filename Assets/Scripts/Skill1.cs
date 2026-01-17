using UnityEngine;
using System.Collections;

// The class name 'Skill1' now matches your file name 'Skill1.cs'
public class Skill1 : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject fireballPrefab;
    public int energyCost = 1;
    public float cooldown = 1.5f;
    public float releaseDelay = 0.2f;

    [Header("References")]
    private Player_Energy energy;
    private PlayerMovement movement;
    private PlayerCombat combat;
    private Camera cam;
    private float nextFireTime;

    void Awake()
    {
        // Finds the components needed for your Unity scripting
        energy = GetComponent<Player_Energy>();
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextFireTime)
        {
            // Checks your custom energy script before allowing the cast
            if (energy != null && energy.UseEnergy(energyCost))
            {
                StartCoroutine(CastRoutine());
            }
        }
    }

    private IEnumerator CastRoutine()
    {
        nextFireTime = Time.time + cooldown;

        // Locks player movement during the casting animation
        if (movement != null) movement.canMove = false;
        
        if (combat != null && combat.anim != null) 
            combat.anim.SetTrigger("CastFireball");

        yield return new WaitForSeconds(releaseDelay);

        SpawnProjectile();

        yield return new WaitForSeconds(0.1f);
        if (movement != null) movement.canMove = true;
    }

    private void SpawnProjectile()
    {
        if (fireballPrefab == null) return;

        // Calculates direction toward the mouse for top-down aiming
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 dir = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Instantiate(fireballPrefab, transform.position, Quaternion.Euler(0, 0, angle));
    }
}