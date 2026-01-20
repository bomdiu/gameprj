using UnityEngine;
using System.Collections;

public class ShockwaveEffect : MonoBehaviour
{
    private ShockwaveSkillData data;
    private float currentRadius = 0f;
    private LineRenderer lineRend;
    private PolygonCollider2D polyCol; 

    private int segments = 60; 

    public void Setup(ShockwaveSkillData skillData)
    {
        data = skillData;
        
        // Init Components
        lineRend = GetComponent<LineRenderer>();
        if (lineRend == null) lineRend = gameObject.AddComponent<LineRenderer>();
        
        polyCol = GetComponent<PolygonCollider2D>();
        if (polyCol == null) polyCol = gameObject.AddComponent<PolygonCollider2D>();
        polyCol.isTrigger = true; 

        // Visual Setup
        lineRend.useWorldSpace = false;
        lineRend.loop = true;
        lineRend.positionCount = segments;
        
        // Use your color / material
        lineRend.material = new Material(Shader.Find("Sprites/Default"));
        lineRend.startColor = data.waveColor;
        lineRend.endColor = data.waveColor;
        
        StartCoroutine(ExpandRoutine());
        
        // Destroy calculation using "expansionSpeed"
        float duration = data.maxRadius / data.expansionSpeed;
        Destroy(gameObject, duration + 0.5f);
    }

    private IEnumerator ExpandRoutine()
    {
        while (currentRadius < data.maxRadius)
        {
            // Logic using "expansionSpeed"
            currentRadius += data.expansionSpeed * Time.deltaTime;
            UpdateVisualsAndHitbox();
            yield return null;
        }
    }

    private void UpdateVisualsAndHitbox()
    {
        // Calculate current width based on progress
        float progress = currentRadius / data.maxRadius;
        float currentWidth = Mathf.Lerp(data.startWidth, data.endWidth, progress);

        lineRend.startWidth = currentWidth;
        lineRend.endWidth = currentWidth;

        Vector3[] linePoints = new Vector3[segments];
        float angleStep = 360f / segments;

        // Draw Visuals
        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            linePoints[i] = new Vector3(Mathf.Sin(angle) * currentRadius, Mathf.Cos(angle) * currentRadius, 0);
        }
        lineRend.SetPositions(linePoints);

        // Update Hitbox (Donut Shape)
        Vector2[] outerPoints = new Vector2[segments];
        Vector2[] innerPoints = new Vector2[segments];
        
        float halfWidth = currentWidth * 0.5f;
        float innerRadius = currentRadius - halfWidth;
        float outerRadius = currentRadius + halfWidth;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float sin = Mathf.Sin(angle);
            float cos = Mathf.Cos(angle);

            outerPoints[i] = new Vector2(sin * outerRadius, cos * outerRadius);
            innerPoints[i] = new Vector2(sin * innerRadius, cos * innerRadius);
        }

        polyCol.pathCount = 2;
        polyCol.SetPath(0, outerPoints);
        polyCol.SetPath(1, innerPoints);
    }

    // --- UPDATED COLLISION LOGIC ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Deal Damage
            PlayerStats playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(data.damage);
            }

            // 2. Apply Knockback
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                // Calculate direction from Center of Wave -> Player
                Vector2 direction = (collision.transform.position - transform.position).normalized;
                
                // Apply physics force
                playerRb.AddForce(direction * data.knockbackForce, ForceMode2D.Impulse);

                // IMPORTANT: We must briefly disable PlayerMovement, otherwise Update() will overwrite this force immediately
                PlayerMovement move = collision.GetComponent<PlayerMovement>();
                if (move != null)
                {
                    StartCoroutine(KnockbackStun(move));
                }
            }
        }
    }

    // Helper to disable movement for a split second so knockback works
    private IEnumerator KnockbackStun(PlayerMovement move)
    {
        move.canMove = false;
        yield return new WaitForSeconds(0.2f); // Stun duration (adjust if needed)
        move.canMove = true;
    }
}