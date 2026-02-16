using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 10f;
    public float lifetime = 5f;

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
    BaseUnit unit;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _boomerangBehavior = GetComponent<BoomerangProjectileBehavior>();
        
        // Ensure Rigidbody2D is properly registered with physics engine
        if (_rb != null)
        {
            // Reset physics state for newly instantiated projectile
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }
    }

    public void Setup(float damage, Vector2 direction, float angleInDegrees, Vector3 originWorldPos, bool isAOE = false, BaseUnit sourceUnit = null)
    {
        _damage = damage;
        _isAoe = isAOE;
        unit = sourceUnit;
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
        // should only hit enemies in the same row pass through enemies in other lanes
        if (other.CompareTag("Enemy") && IsSameRow(other.transform))
        {
            if (_boomerangBehavior != null && _boomerangBehavior.HandleEnemyTrigger(other))
            {
                return;
            }

            // Deal 5 damage every time we hit an enemy
            TargetDummyTest enemy = other.GetComponent<TargetDummyTest>();
            if (_isAoe)
            {
                // If it's an AOE projectile, we want to hit all enemies in a radius
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f); // Adjust radius as needed 1f is just an example
                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("Enemy") && IsSameRow(hit.transform))
                    {
                        TargetDummyTest aoeEnemy = hit.GetComponent<TargetDummyTest>();
                        if (aoeEnemy != null)
                        {
                            aoeEnemy.TakeDamage(Mathf.RoundToInt(_damage)); //use passed damage
                            ApplySlowIfConfigured(aoeEnemy);
                        }
                    }
                }
            }
            if (!_isAoe && enemy != null)
            {
                enemy.TakeDamage(Mathf.RoundToInt(_damage)); //use passed damage
                ApplySlowIfConfigured(enemy);
            }
            unit?.onHit();
            Destroy(gameObject);
        }
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
            if (hit == null || !hit.CompareTag("Enemy")) continue;

            float sqrDistance = (hit.transform.position - transform.position).sqrMagnitude;
            if (sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestTarget = hit.transform;
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

    private void ApplySlowIfConfigured(TargetDummyTest enemy)
    {
        if (!_applySlowOnHit || enemy == null) return;
        enemy.ApplySlow(_slowPercent, _slowDuration);
    }
}
