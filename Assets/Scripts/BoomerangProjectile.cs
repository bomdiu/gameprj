using UnityEngine;
using System.Collections.Generic;

public class BoomerangChain : MonoBehaviour
{
    private enum State { Launching, Tracking, Returning }
    private State currentState = State.Launching;

    private Transform playerTransform;
    private float moveSpeed;
    private int damage;
    private LayerMask enemyLayer;
    
    private int totalReachesPerformed = 0; // Total times we reached ANY target
    private int maxBouncesPerEnemy = 3;
    private int requiredTotalReaches; 
    
    private Transform currentTarget;
    private Vector3 initialTargetPos;
    
    private Dictionary<Transform, int> hitCounts = new Dictionary<Transform, int>(); 
    private Transform lastHitTarget;
    private bool hasDamagedCurrentTarget = false;

    private Vector3 currentVelocity; 
    [SerializeField] private float curveSmoothness = 0.15f; 

    public void Setup(float speed, Transform player, int dmg, LayerMask mask, int enemyCount, Vector3 mouseTarget)
    {
        moveSpeed = speed;
        playerTransform = player;
        damage = dmg;
        enemyLayer = mask;
        initialTargetPos = mouseTarget;

        // --- THE MATH FIX ---
        // For 1 enemy: (3 hits) + (2 bounces back to player) = 5 reaches
        // For N enemies: (N * 3 hits)
        if (enemyCount <= 1) {
            requiredTotalReaches = (maxBouncesPerEnemy * 2) - 1; 
        } else {
            requiredTotalReaches = enemyCount * maxBouncesPerEnemy;
        }

        GetComponent<Collider2D>().isTrigger = true;
    }

    void Update()
    {
        if (playerTransform == null) { Destroy(gameObject); return; }

        Vector3 targetPos = transform.position;

        switch (currentState)
        {
            case State.Launching:
                targetPos = initialTargetPos;
                if (Vector2.Distance(transform.position, initialTargetPos) < 0.6f)
                {
                    FindNextTarget();
                    currentState = State.Tracking;
                }
                break;

            case State.Tracking:
                if (currentTarget != null)
                {
                    targetPos = currentTarget.position;
                    if (Vector2.Distance(transform.position, currentTarget.position) < 0.6f)
                    {
                        ProcessReach(currentTarget);
                    }
                }
                else
                {
                    currentState = State.Returning;
                }
                break;

            case State.Returning:
                targetPos = playerTransform.position;
                if (Vector2.Distance(transform.position, playerTransform.position) < 0.6f)
                {
                    RemoveBoomerang();
                }
                break;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, curveSmoothness, moveSpeed);
    }

    private void FindNextTarget()
    {
        hasDamagedCurrentTarget = false;
        
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 15f, enemyLayer);
        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        // 1. Look for enemies that still need hits
        foreach (var hit in hitColliders)
        {
            Transform t = hit.transform;
            if (t == lastHitTarget) continue;

            int count = 0;
            hitCounts.TryGetValue(t, out count);

            if (count < maxBouncesPerEnemy)
            {
                float dist = Vector2.Distance(transform.position, t.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    bestTarget = t;
                }
            }
        }

        // 2. If no enemy target found but we aren't done yet, bounce to player
        if (bestTarget == null && totalReachesPerformed < requiredTotalReaches)
        {
            if (lastHitTarget != playerTransform)
            {
                bestTarget = playerTransform;
            }
        }

        currentTarget = bestTarget;
        if (currentTarget == null) currentState = State.Returning;
    }

   private void ProcessReach(Transform target)
    {
        totalReachesPerformed++;
        lastHitTarget = target;

        // If it's an enemy, deal damage
        if (target != playerTransform && !hasDamagedCurrentTarget)
        {
            // 1. TRY REGULAR ENEMY
            Enemy_Health health = target.GetComponent<Enemy_Health>();
            // 2. TRY BOSS [ADDED]
            BossHealth bossHealth = target.GetComponent<BossHealth>();

            if (health != null)
            {
                health.TakeDamage(damage, DamageType.NormalAttack, false);
                hasDamagedCurrentTarget = true;
            }
            else if (bossHealth != null) // [ADDED BOSS LOGIC]
            {
                bossHealth.TakeDamage(damage);
                hasDamagedCurrentTarget = true;
            }
            
            if (!hitCounts.ContainsKey(target)) hitCounts[target] = 0;
            hitCounts[target]++;
        }

        // Logic check: Are we done with all required bounces?
        if (totalReachesPerformed >= requiredTotalReaches) 
        { 
            currentState = State.Returning; 
            currentTarget = null; 
        }
        else 
        { 
            FindNextTarget(); 
        }
    }

    private void RemoveBoomerang()
    {
        ParticleSystem particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null)
        {
            particles.Stop();
            particles.transform.SetParent(null);
            Destroy(particles.gameObject, 2f);
        }
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform == currentTarget)
        {
            ProcessReach(collision.transform);
        }
    }
}