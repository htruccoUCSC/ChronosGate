using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float lifetime = 5f;

    [Header("Collision")]
    [SerializeField] private LayerMask enemyMask; // set to Enemies in inspector (or auto-filled in Awake)

    // we still keep _damage because your existing system calls Setup(damage, ...)
    // BUT for this assignment requirement we will force hits to deal 5 damage on enemies
    private float _damage;
    private bool _isAoe;
    private Rigidbody2D _rb;
    private int _originRow;
    private bool _hasOriginRow;
    private bool _retargetAtApex;
    private bool _didApexRetarget;
    private bool _ignoreRowCheck;
    private float _retargetRadius = 20f;
    private LayerMask _retargetMask;
    private float _previousVerticalSpeed;
    private BoomerangProjectileBehavior _boomerangBehavior;
    private bool _applySlowOnHit;
    private float _slowPercent;
    private float _slowDuration;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _boomerangBehavior = GetComponent<BoomerangProjectileBehavior>();

        // if not set in inspector, default it to the Enemies layer
        if (enemyMask.value == 0)
        {
            enemyMask = LayerMask.GetMask("Enemies");
        }
    }

    public void Setup(float damage, Vector2 direction, float angleInDegrees, Vector3 originWorldPos, bool isAOE = false)
    {
        _damage = damage;
        _isAoe = isAOE;
        CacheOriginRow(originWorldPos);
        _didApexRetarget = false;
        _previousVerticalSpeed = 0f;
        Destroy(gameObject, lifetime);

        // straight shot
        if (angleInDegrees <= 0)
        {
            // no gravity
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;

            // fully horizontal velocity
            _rb.linearVelocity = direction * speed;

            // rotates the visual but we should just make our sprites face right by default and avoid this dumbness
            transform.rotation = Quaternion.identity;
        }
        // catapult shot
        else
        {
            // yes gravity
            _rb.bodyType = RigidbodyType2D.Dynamic;
            // very heavy gravity, it looks better this way
            _rb.gravityScale = 3f;

            // calculate launch direction with angle
            float directionSign = Mathf.Sign(direction.x);
            Vector2 launchDir = Quaternion.Euler(0, 0, angleInDegrees * directionSign) * Vector2.right;

            // another visual rotation
            transform.rotation = Quaternion.Euler(0, 0, angleInDegrees);

            // initial velocity is straight along the launch angle
            _rb.linearVelocity = launchDir * speed;
        }

        _previousVerticalSpeed = _rb.linearVelocity.y;
    }

    public void EnableApexRetarget(LayerMask targetMask, float retargetRadius = 20f, bool ignoreRowCheck = true)
    {
        _retargetAtApex = true;
        _retargetMask = targetMask;
        _retargetRadius = Mathf.Max(0.1f, retargetRadius);
        _ignoreRowCheck = ignoreRowCheck;
    }

    public void EnableOnHitSlow(float slowPercent, float duration)
    {
        _applySlowOnHit = true;
        _slowPercent = Mathf.Clamp(slowPercent, 0f, 0.95f);
        _slowDuration = Mathf.Max(0f, duration);
    }

    void Update()
    {
        if (!_retargetAtApex || _didApexRetarget || _rb == null) return;
        if (_rb.bodyType != RigidbodyType2D.Dynamic) return;

        float currentVerticalSpeed = _rb.linearVelocity.y;
        if (_previousVerticalSpeed > 0f && currentVerticalSpeed <= 0f)
        {
            RetargetFromApex();
        }

        _previousVerticalSpeed = currentVerticalSpeed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only hit objects on the enemy layer mask (no hardcoded enemy types)
        if (((1 << other.gameObject.layer) & enemyMask.value) == 0) return;

        // should only hit enemies in the same row pass through enemies in other lanes
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        if (enemy == null) return;

        // IMPORTANT: do row check using the enemy root transform (not the collider child)
        if (!IsSameRow(enemy.transform)) return;

        if (_boomerangBehavior != null && _boomerangBehavior.HandleEnemyTrigger(other))
        {
            return;
        }

        if (_isAoe)
        {
            // If it's an AOE projectile, we want to hit all enemies in a radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, enemyMask); // uses enemy layer mask
            foreach (Collider2D hit in hits)
            {
                BaseEnemy aoeEnemy = hit.GetComponentInParent<BaseEnemy>();
                if (aoeEnemy == null) continue;

                if (!IsSameRow(aoeEnemy.transform)) continue;

                aoeEnemy.TakeDamage(Mathf.RoundToInt(_damage)); //use passed damage
                ApplySlowIfConfigured(aoeEnemy);
            }
        }
        else
        {
            enemy.TakeDamage(Mathf.RoundToInt(_damage)); //use passed damage
            ApplySlowIfConfigured(enemy);
        }

        Destroy(gameObject);
    }

    private bool IsSameRow(Transform target)
    {
        if (_ignoreRowCheck) return true;

        Tilemap tilemap = WaveManager.Instance != null ? WaveManager.Instance.tilemap : null;
        if (tilemap == null || !_hasOriginRow) return true;

        int targetRow = tilemap.WorldToCell(target.position).y;
        return targetRow == _originRow;
    }

    private void CacheOriginRow(Vector3 originWorldPos)
    {
        Tilemap tilemap = WaveManager.Instance != null ? WaveManager.Instance.tilemap : null;
        if (tilemap == null)
        {
            _hasOriginRow = false;
            return;
        }

        _originRow = tilemap.WorldToCell(originWorldPos).y;
        _hasOriginRow = true;
    }

    private void RetargetFromApex()
    {
        Transform newTarget = FindRetargetTarget();
        if (newTarget == null) return;

        Vector2 toTarget = (newTarget.position - transform.position);
        if (toTarget.sqrMagnitude <= 0.0001f) return;

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.linearVelocity = toTarget.normalized * speed;
        _didApexRetarget = true;
    }

    private Transform FindRetargetTarget()
    {
        Collider2D[] hits = _retargetMask.value != 0
            ? Physics2D.OverlapCircleAll(transform.position, _retargetRadius, _retargetMask)
            : Physics2D.OverlapCircleAll(transform.position, _retargetRadius);

        Transform closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            // only consider enemies by layer mask (not tag)
            if (((1 << hit.gameObject.layer) & enemyMask.value) == 0) continue;

            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null) continue;

            float sqrDistance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestTarget = enemy.transform;
            }
        }

        return closestTarget;
    }

    public float Damage => _damage;

    public Rigidbody2D Body => _rb;

    public void DisableApexRetarget()
    {
        _retargetAtApex = false;
    }

    private void ApplySlowIfConfigured(BaseEnemy enemy)
    {
        if (!_applySlowOnHit || enemy == null) return;
        enemy.ApplySlow(_slowPercent, _slowDuration);
    }
}
