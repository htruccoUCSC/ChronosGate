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
    private bool _ignoreRowCheck;
    private bool _applySlowOnHit;
    private float _slowPercent;
    private float _slowDuration;
    private bool _requireDesignatedTargetHit;
    private Transform _designatedTarget;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(float damage, Vector2 direction, float angleInDegrees, Vector3 originWorldPos, bool isAOE = false)
    {
        _damage = damage;
        _isAoe = isAOE;
        _requireDesignatedTargetHit = false;
        _designatedTarget = null;
        CacheOriginRow(originWorldPos);
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
    }

    public void SetIgnoreRowCheck(bool ignoreRowCheck)
    {
        _ignoreRowCheck = ignoreRowCheck;
    }

    public void SetDesignatedTarget(Transform target)
    {
        _designatedTarget = target;
        _requireDesignatedTargetHit = target != null;
    }

    public void EnableOnHitSlow(float slowPercent, float duration)
    {
        _applySlowOnHit = true;
        _slowPercent = Mathf.Clamp(slowPercent, 0f, 0.95f);
        _slowDuration = Mathf.Max(0f, duration);
    }

    void Update()
    {
        if (_requireDesignatedTargetHit && _designatedTarget == null)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // should only hit enemies in the same row pass through enemies in other lanes
        if (other.CompareTag("Enemy") && IsSameRow(other.transform))
        {
            if (_requireDesignatedTargetHit && !IsDesignatedTargetCollider(other))
            {
                return;
            }
            // Deal 5 damage every time we hit an enemy
            TargetDummyTest enemy = other.GetComponentInParent<TargetDummyTest>();
            if (_isAoe)
            {
                // If it's an AOE projectile, we want to hit all enemies in a radius
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f); // Adjust radius as needed 1f is just an example
                foreach (Collider2D hit in hits)
                {
                    if (hit.CompareTag("Enemy") && IsSameRow(hit.transform))
                    {
                        TargetDummyTest aoeEnemy = hit.GetComponentInParent<TargetDummyTest>();
                        if (aoeEnemy != null)
                        {
                            aoeEnemy.TakeDamage(Mathf.RoundToInt(_damage)); //use passed damage
                            ApplySlowIfEnabled(aoeEnemy);
                        }
                    }
                }
            }
            if (!_isAoe && enemy != null)
            {
                enemy.TakeDamage(Mathf.RoundToInt(_damage)); //use passed damage
                ApplySlowIfEnabled(enemy);
            }

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

    private void ApplySlowIfEnabled(TargetDummyTest enemy)
    {
        if (!_applySlowOnHit || enemy == null)
        {
            return;
        }

        enemy.ApplySlow(_slowPercent, _slowDuration);
    }

    private bool IsDesignatedTargetCollider(Collider2D other)
    {
        if (_designatedTarget == null || other == null)
        {
            return false;
        }

        Transform hitTransform = other.transform;
        return hitTransform == _designatedTarget
               || hitTransform.IsChildOf(_designatedTarget)
               || _designatedTarget.IsChildOf(hitTransform);
    }
}
