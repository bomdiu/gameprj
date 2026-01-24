using UnityEngine;
using System.Collections.Generic;

public class ObjectFader : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeAlpha = 0.4f;
    public float fadeSpeed = 3f;

    private SpriteRenderer spriteRenderer;
    private float targetAlpha = 1.0f;
    
    // Track how many valid objects are currently behind the tree
    private List<Collider2D> objectsBehind = new List<Collider2D>();

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Color curColor = spriteRenderer.color;
        float newAlpha = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = new Color(curColor.r, curColor.g, curColor.b, newAlpha);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Add Player, Enemy, or Projectiles to the list
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
        {
            if (!objectsBehind.Contains(other))
            {
                objectsBehind.Add(other);
            }
            targetAlpha = fadeAlpha;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (objectsBehind.Contains(other))
        {
            objectsBehind.Remove(other);
        }

        // Only return to solid color if NO relevant objects are left behind the tree
        if (objectsBehind.Count == 0)
        {
            targetAlpha = 1.0f;
        }
    }
}