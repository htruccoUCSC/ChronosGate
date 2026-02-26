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
    private bool _ampOnHitBool;
    private float _ampOnHitAmount;
    private float _slowPercent;
    private float _slowDuration;
    private BaseUnit _owner;
    [Header("Penetration")]
    [Tooltip("If true, projectile will not be destroyed when hitting an enemy and will pass through.")]
    public bool passThroughEnemies = false;
    [Tooltip("If > 0 and passThroughEnemies is true, projectile will be destroyed after hitting this many enemies.")]
    public int maxPenetration = 0;
    private int _penetrationCount = 0;

    // this is mainly for stuff like trebuchet/wizard where we already picked a target
    private Transform _designatedTarget;

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

    public void Setup(float damage, Vector2 direction, float angleInDegrees, Vector3 originWorldPos, bool isAOE = false, BaseUnit owner = null)
    {
        _damage = damage;
        _isAoe = isAOE;
        _owner = owner;
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

    // older code in Units was calling Setup with 6 args, so we keep that alive too
    public void Setup(float damage, Vector2 direction, float angleInDegrees, Vector3 originWorldPos, bool isAOE, bool ignoreRowCheck, BaseUnit owner = null)
    {
        _ignoreRowCheck = ignoreRowCheck;
        Setup(damage, direction, angleInDegrees, originWorldPos, isAOE, owner);
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
  public void EnableOnHitAmp(float ampPercent)
    {
        _ampOnHitBool = true;
        _ampOnHitAmount=ampPercent;

    }
    // some unit scripts expect these exact method names
    public void SetIgnoreRowCheck(bool ignore)
    {
        _ignoreRowCheck = ignore;
    }

    public void SetDesignatedTarget(Transform target)
    {
        _designatedTarget = target;
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
        // Only hit objects on the enemy layer mask (no hardcoded enemy types unless i get lazy)
        if (((1 << other.gameObject.layer) & enemyMask.value) == 0) return;

        // try BaseEnemy first (new), otherwise TargetDummyTest (older)
        BaseEnemy enemy = other.GetComponentInParent<BaseEnemy>();
        _owner.RecentlyHit(enemy);
        TargetDummyTest testEnemy = null;

        if (enemy == null)
        {
            testEnemy = other.GetComponentInParent<TargetDummyTest>();
            if (testEnemy == null) return;
        }

        // row check using the enemy root transform (not the collider child)
        Transform enemyRoot = (enemy != null) ? enemy.transform : testEnemy.transform;
        if (!IsSameRow(enemyRoot)) return;

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
                if (hit == null) continue;

                BaseEnemy aoeEnemy = hit.GetComponentInParent<BaseEnemy>();
                BaseEnemy aoeTestEnemy = null;
               
                if (aoeEnemy == null)
                {
                    aoeTestEnemy = hit.GetComponentInParent<BaseEnemy>();
                    if (aoeTestEnemy == null) continue;
                }

                Transform root = (aoeEnemy != null) ? aoeEnemy.transform : aoeTestEnemy.transform;
                if (!IsSameRow(root)) continue;

                int dealt = Mathf.RoundToInt(_damage);

                if (aoeEnemy != null)
                {   
                    _owner.OnHit();
                  
                    int damage = Mathf.RoundToInt(dealt*aoeEnemy.DamageAmp);
                    aoeEnemy.TakeDamage(_owner, damage);

                    if (_ampOnHitBool)
                {
                    ApplyAmpIfConfigured(enemy);
                }
                    ApplySlowIfConfigured(aoeEnemy);
                }
                else
                {
                     _owner.OnHit();
                      int damage = Mathf.RoundToInt(dealt*aoeEnemy.DamageAmp);
                    aoeTestEnemy.TakeDamage(_owner, damage);

                    if (_ampOnHitBool)
                {
                    ApplyAmpIfConfigured(enemy);
                }
                    ApplySlowIfConfigured(aoeTestEnemy);
                }
            }
        }
        else
        {
            int dealt = Mathf.RoundToInt(_damage);

            if (enemy != null)
            {
                _owner.OnHit();
                 int damage = Mathf.RoundToInt(dealt*enemy.DamageAmp);
                   Debug.Log(dealt+"x Amp "+enemy.DamageAmp+" = "+damage);
                enemy.TakeDamage(_owner, damage);
                if (_ampOnHitBool)
                {
                    ApplyAmpIfConfigured(enemy);
                }
                ApplySlowIfConfigured(enemy);
            }
            else
            {
                _owner.OnHit();
                
                testEnemy.TakeDamage(dealt, _owner);
                if (_ampOnHitBool)
                {
                    ApplyAmpIfConfigured(enemy);
                }
                ApplySlowIfConfigured(testEnemy);
                
            }
        }

        // Destroy behavior: if projectile is configured to pass through enemies, don't auto-destroy
        if (_isAoe)
        {
            Destroy(gameObject);
            return;
        }

        if (passThroughEnemies)
        {
            if (maxPenetration > 0)
            {
                _penetrationCount++;
                if (_penetrationCount >= maxPenetration)
                {
                    Destroy(gameObject);
                }
            }
            // else: infinite penetration, only destroyed by lifetime or off-screen
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        // Ensure projectiles are cleaned up when they leave the camera view
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
        // if something already picked a target, just use it
        if (_designatedTarget != null) return _designatedTarget;

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
            TargetDummyTest testEnemy = null;

            Transform root;
            if (enemy != null) root = enemy.transform;
            else
            {
                testEnemy = hit.GetComponentInParent<TargetDummyTest>();
                if (testEnemy == null) continue;
                root = testEnemy.transform;
            }

            float sqrDistance = (root.position - transform.position).sqrMagnitude;
            if (sqrDistance < closestDistance)
            {
                closestDistance = sqrDistance;
                closestTarget = root;
            }
        }

        return closestTarget;
    }

    public float Damage => _damage;
    public BaseUnit Owner => _owner;

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

      private void ApplyAmpIfConfigured(BaseEnemy enemy)
    {
        if (!_applySlowOnHit || enemy == null) return;
         enemy.ApplyDebuff(
        BaseEnemy.DebuffType.DamageAmp,
       _ampOnHitAmount,
        DebuffDuration.AmpDuration,
        enemy.ApplyAmp
    );
    }

    private void ApplySlowIfConfigured(TargetDummyTest enemy)
    {
        if (!_applySlowOnHit || enemy == null) return;
        enemy.ApplySlow(_slowPercent, _slowDuration);
    }
}
