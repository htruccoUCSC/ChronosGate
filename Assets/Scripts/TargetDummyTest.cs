using UnityEngine;
using System.Collections;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f; // how fast it moves left
    private float m_SlowMultiplier = 1f;
    private float m_SlowTimeRemaining;

    [Header("Health")]
    public int maxHealth = 50;
    private int currentHealth;

    private bool registered = false;
    private bool alreadyCountedAsEscape = false; // prevents double life loss
    private int polymorphVersion = 0;
    private SpriteRenderer cachedRenderer;
    private Sprite baseSprite;
    private Collider2D enemyCollider;
    [Header("Debug")]
    public bool showHitbox = true; // default off

    void Start()
    {
        currentHealth = maxHealth;
        enemyCollider = GetComponent<Collider2D>();
        if (enemyCollider == null)
        {
            enemyCollider = GetComponentInChildren<Collider2D>();
        }
        cachedRenderer = GetComponentInChildren<SpriteRenderer>();
        if (cachedRenderer != null)
        {
            baseSprite = cachedRenderer.sprite;
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterEnemy(this);
            registered = true;
        }
        else
        {
            Debug.LogError("WaveManager.Instance is NULL (WaveManager not in scene).");
        }
    }

    void Update()
    {
        transform.position += Vector3.left * moveSpeed * m_SlowMultiplier * Time.deltaTime;

        if (m_SlowTimeRemaining > 0f)
        {
            m_SlowTimeRemaining -= Time.deltaTime;
            if (m_SlowTimeRemaining <= 0f)
            {
                m_SlowTimeRemaining = 0f;
                m_SlowMultiplier = 1f;
            }
        }
        // if crossed the left end of the map, lose a life once
        if (!alreadyCountedAsEscape && WaveManager.Instance != null)
        {
            float loseX = WaveManager.Instance.GetLoseLineX();
            float enemyLeftX = enemyCollider != null ? enemyCollider.bounds.min.x : transform.position.x;
            if (enemyLeftX <= loseX)
            {
                alreadyCountedAsEscape = true;
                WaveManager.Instance.EnemyReachedEnd(this);
            }
        }
    }

    public void TakeDamage(int damage, BaseUnit attacker = null)
    {
        currentHealth -= damage;
        Debug.Log($"Target Dummy took {damage} damage, current health: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            if (attacker != null)
            {
                attacker.OnKill();
            }
            Destroy(gameObject);
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        float clampedPercent = Mathf.Clamp(slowPercent, 0f, 0.95f);
        float newMultiplier = 1f - clampedPercent;

        m_SlowMultiplier = Mathf.Min(m_SlowMultiplier, newMultiplier);
        m_SlowTimeRemaining = Mathf.Max(m_SlowTimeRemaining, Mathf.Max(0f, duration));
    }

    public void ApplyPolymorph(Sprite sheepSprite, float duration)
    {
        if (cachedRenderer == null || sheepSprite == null)
        {
            return;
        }

        int version = ++polymorphVersion;
        cachedRenderer.sprite = sheepSprite;

        StartCoroutine(RemovePolymorphAfterDuration(version, Mathf.Max(0f, duration)));
    }

    private IEnumerator RemovePolymorphAfterDuration(int version, float duration)
    {
        if (duration > 0f)
        {
            yield return new WaitForSeconds(duration);
        }

        if (version != polymorphVersion)
        {
            yield break;
        }

        if (cachedRenderer != null)
        {
            cachedRenderer.sprite = baseSprite;
        }
    }

    private void OnDestroy()
    {
        if (registered && WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
            registered = false;
        }
    }
void OnDrawGizmos()
{
    if (!showHitbox) return;

    // Look for collider on self or children
    Collider2D col = GetComponent<Collider2D>();
    if (col == null) col = GetComponentInChildren<Collider2D>();
    if (col == null) return;

    Bounds bounds = col.bounds;
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireCube(bounds.center, bounds.size);
}


    
}
