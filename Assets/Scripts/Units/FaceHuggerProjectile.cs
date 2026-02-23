using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class FaceHuggerProjectile : MonoBehaviour
{
    public event Action OnFaceHuggerDestroyed;

    private float speed = 3f;
    private Rigidbody2D rb;
    private LayerMask enemyMask;

    private float damage;
    private float slowPercent;
    private float damagePerSecond;
    private bool isAttached = false;
    private BaseEnemy attachedEnemy;
    private float damageTimer = 0f;
    private float slowRefreshTimer = 0f;
    private const float SLOW_REFRESH_INTERVAL = 0.5f; // Refresh slow every 0.5 seconds
    
    private int originRow;
    private bool hasOriginRow;
    
    private BaseUnit owner;
    private SpriteRenderer spriteRenderer;
    
    // Manual timeout tracking instead of Destroy(gameObject, time)
    private float timeAlive = 0f;
    private const float DESTROY_TIMEOUT = 10f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        // Get the sprite renderer to control sorting order
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public void Initialize(
        float damage,
        float speed,
        float slowPercent,
        float damagePerSecond,
        Vector3 originPos,
        LayerMask enemyMask,
        BaseUnit owner)
    {
        this.damage = damage;
        this.speed = speed;
        this.slowPercent = slowPercent;
        this.damagePerSecond = damagePerSecond;
        this.enemyMask = enemyMask;
        this.owner = owner;

        CacheOriginRow(originPos);

        rb.linearVelocity = Vector2.right * speed;
    }

    // Cache the row of the origin position to optimize same-row checks later
    // This is so extra, but its an example of what we can do in the future if we need to find performance improvements
    private void CacheOriginRow(Vector3 originWorldPos)
    {
        if (WaveManager.Instance == null || WaveManager.Instance.tilemap == null)
        {
            hasOriginRow = false;
            return;
        }

        originRow = WaveManager.Instance.tilemap.WorldToCell(originWorldPos).y;
        hasOriginRow = true;
    }

    private void Update()
    {
        if (isAttached)
        {
            if (attachedEnemy != null)
            {
                // Follow the enemy's position with slight offset above
                Vector3 offset = Vector3.up * 0.1f; // Slight vertical offset
                transform.position = attachedEnemy.transform.position + offset;

                // Continuously refresh the slow effect
                slowRefreshTimer += Time.deltaTime;
                if (slowRefreshTimer >= SLOW_REFRESH_INTERVAL)
                {
                    // Reapply slow with a short duration to maintain continuous effect
                    attachedEnemy.ApplySlow(slowPercent, SLOW_REFRESH_INTERVAL + 0.1f);
                    slowRefreshTimer = 0f;
                }

                // Apply damage over time
                damageTimer += Time.deltaTime;
                if (damageTimer >= 1f)
                {
                    int damageAmount = Mathf.RoundToInt(damagePerSecond);
                    attachedEnemy.TakeDamage(damageAmount);
                    damageTimer -= 1f;
                }
            }
            else
            {
                // Enemy died or was destroyed, destroy the facehugger
                Debug.Log("[FaceHuggerProjectile] Attached enemy destroyed, removing facehugger");
                DestroyFaceHugger();
            }
        }
        else
        {
            // Only track timeout when NOT attached
            timeAlive += Time.deltaTime;
            if (timeAlive >= DESTROY_TIMEOUT)
            {
                Debug.Log("[FaceHuggerProjectile] Timeout reached without attaching, destroying");
                DestroyFaceHugger();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isAttached) return;

        if (((1 << other.gameObject.layer) & enemyMask.value) == 0) return;

        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null) return;

        if (!IsSameRow(enemy.transform)) return;

        AttachToEnemy(enemy);
    }

    // Same row helper function
    private bool IsSameRow(Transform target)
    {
        if (!hasOriginRow) return true;

        if (WaveManager.Instance == null || WaveManager.Instance.tilemap == null)
        {
            return true;
        }

        int targetRow = WaveManager.Instance.tilemap.WorldToCell(target.position).y;
        return targetRow == originRow;
    }

    private void AttachToEnemy(BaseEnemy enemy)
    {
        isAttached = true;
        attachedEnemy = enemy;

        // Stop moving
        rb.linearVelocity = Vector2.zero;

        // Apply initial slow
        enemy.ApplySlow(slowPercent, SLOW_REFRESH_INTERVAL + 0.1f);

        // Apply initial burst damage
        int initialDamage = Mathf.RoundToInt(damage);
        enemy.TakeDamage(initialDamage);

        // Make the facehugger smaller when attached
        transform.localScale *= 0.5f;

        // Ensure facehugger renders on top of the enemy
        SetRenderingOrderAboveEnemy(enemy);
    }

    // Adjust the sprite renderer's sorting order to ensure the facehugger appears above the enemy
    private void SetRenderingOrderAboveEnemy(BaseEnemy enemy)
    {
        if (spriteRenderer == null) return;

        // Get the enemy's sprite renderer
        SpriteRenderer enemySpriteRenderer = enemy.GetComponentInChildren<SpriteRenderer>();
        if (enemySpriteRenderer == null)
        {
            enemySpriteRenderer = enemy.GetComponent<SpriteRenderer>();
        }

        if (enemySpriteRenderer != null)
        {
            // Set facehugger to render on the same sorting layer but higher order
            spriteRenderer.sortingLayerName = enemySpriteRenderer.sortingLayerName;
            spriteRenderer.sortingOrder = enemySpriteRenderer.sortingOrder + 1;
        }
        else
        {
            // Fallback magic number solution of doom and despair
            spriteRenderer.sortingOrder += 10;
        }

        // Also adjust z-position slightly forward (towards camera)
        Vector3 pos = transform.position;
        pos.z = enemy.transform.position.z - 0.1f;
        transform.position = pos;
    }

    private void DestroyFaceHugger()
    {
        OnFaceHuggerDestroyed?.Invoke();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        OnFaceHuggerDestroyed?.Invoke();
    }

    // Debugging visualization
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (isAttached)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.15f);
        }
    }
}
