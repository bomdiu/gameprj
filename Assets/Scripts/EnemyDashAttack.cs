using UnityEngine;
using System.Collections;

public class EnemyAlertDash : MonoBehaviour
{
    [Header("Range")]
    public float detectRange = 3f;
    public float attackRange = 1.2f;

    [Header("Alert")]
    public GameObject alertPrefab;
    public float alertTime = 0.35f;

    [Header("Dash")]
    public float dashSpeed = 8f;
    public float dashDuration = 0.2f;

    [Header("Cooldown")]
    public float attackCooldown = 1.5f;

    private Transform player;
    private Animator anim;

    private bool isBusy;       // đang alert / dash / cooldown
    private GameObject currentAlert;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (isBusy) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectRange)
        {
            StartCoroutine(AlertDashAttack());
        }
    }

    IEnumerator AlertDashAttack()
    {
        isBusy = true;

        // ===== ALERT =====
        currentAlert = Instantiate(
            alertPrefab,
            transform.position + Vector3.up * 0.6f,
            Quaternion.identity,
            transform
        );

        yield return new WaitForSeconds(alertTime);

        // ===== DASH =====
        if (currentAlert != null)
            Destroy(currentAlert);

        anim.SetBool("isDashing", true);

        Vector2 dir = (player.position - transform.position).normalized;
        float timer = 0f;

        while (timer < dashDuration)
        {
            transform.position += (Vector3)(dir * dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        anim.SetBool("isDashing", false);

        // ===== ATTACK =====
        anim.SetTrigger("Attacking");

        // đợi attack anim + hitbox xử lý
        yield return new WaitForSeconds(0.4f);

        // ===== COOLDOWN =====
        yield return new WaitForSeconds(attackCooldown);

        isBusy = false;
    }
}
