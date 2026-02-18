using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseEnemy : MonoBehaviour
{
    // my take on wave scaling
    // Waves 1..START_AFTER_WAVE: base stats (no scaling)
    // After that, stats scale up once every SCALE_EVERY_N_WAVES.
    // Example: START_AFTER_WAVE = 3 and SCALE_EVERY_N_WAVES = 3
    // Waves 1-3: base stats
    // Waves 4-6: +10% once
    // Waves 7-9: +10% twice, etc.
    private const int START_AFTER_WAVE = 3;
    private const int SCALE_EVERY_N_WAVES = 3;   // change to 5 later if you want
    private const float SCALE_PER_STEP = 0.10f;  // 10% per step
    private const bool COMPOUND = true;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 0.5f; // how fast it moves left
    protected float m_SlowMultiplier = 1f;
    protected float m_SlowTimeRemaining;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 50;
    protected int currentHealth;

    [Header("Melee vs Troops")]
    [SerializeField] protected float damagePerSecond = 5f;

    protected BaseUnit contactTroop;
    protected bool isAttackingTroop;

    protected bool alreadyCountedAsEscape = false; // prevents double life loss

    protected Rigidbody2D rb;

    // damage accumulator so float DPS works with int HP
    private float damageCarry = 0f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
    }

    protected virtual void Start()
    {
        ApplyWaveScaling();

        currentHealth = maxHealth;

        // Register if a WaveManager exists (not required for BaseEnemy to function)
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterEnemy(this);
        }
    }

    protected virtual void FixedUpdate()
    {
        // move only when not attacking a troop
        if (!isAttackingTroop)
        {
            Vector2 move = Vector2.left * moveSpeed * m_SlowMultiplier * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }
    }

    protected virtual void Update()
    {
        // slow timer
        if (m_SlowTimeRemaining > 0f)
        {
            m_SlowTimeRemaining -= Time.deltaTime;
            if (m_SlowTimeRemaining <= 0f)
            {
                m_SlowTimeRemaining = 0f;
                m_SlowMultiplier = 1f;
            }
        }

        // deal damage continuously while in contact
        if (isAttackingTroop)
        {
            if (contactTroop == null || contactTroop.IsDead)
            {
                contactTroop = null;
                isAttackingTroop = false;
            }
            else
            {
                // damage per second, frame-rate independent
                float dmgThisFrame = damagePerSecond * Time.deltaTime;


                ApplyContinuousDamageAsInt(dmgThisFrame);
            }
        }
        // check if it has reached the left end of the map (lose condition)
        if (!alreadyCountedAsEscape && WaveManager.Instance != null)
        {
            float loseX = WaveManager.Instance.GetLoseLineX();
            if (transform.position.x <= loseX)
            {
                alreadyCountedAsEscape = true;
                WaveManager.Instance.EnemyReachedEnd(this);
            }
        }
    }

    private void ApplyWaveScaling()
    {
        if (WaveManager.Instance == null) return;

        int wave = WaveManager.Instance.currentWave;

        // no scaling on waves 1..START_AFTER_WAVE
        if (wave <= START_AFTER_WAVE) return;

        // how many waves past the "no scale" period?
        int wavesPast = wave - START_AFTER_WAVE; // wave 4 => 1

        // scale happens in chunks of SCALE_EVERY_N_WAVES
        // wave 4..6 => steps = 1
        // wave 7..9 => steps = 2
        int steps = Mathf.CeilToInt(wavesPast / (float)SCALE_EVERY_N_WAVES);

        float multiplier = COMPOUND
            ? Mathf.Pow(1f + SCALE_PER_STEP, steps)
            : 1f + (SCALE_PER_STEP * steps);

        maxHealth = Mathf.RoundToInt(maxHealth * multiplier);
        moveSpeed *= multiplier;
        damagePerSecond *= multiplier;

        // Debug.Log($"[BaseEnemy] Wave {wave} steps={steps} multiplier={multiplier:F3}");
    }

    private void ApplyContinuousDamageAsInt(float dmgFloat)
    {
        damageCarry += dmgFloat;
        int whole = Mathf.FloorToInt(damageCarry);
        if (whole > 0)
        {
            damageCarry -= whole;
            contactTroop.TakeDamage(whole);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void ApplySlow(float slowPercent, float duration)
    {
        float clampedPercent = Mathf.Clamp(slowPercent, 0f, 0.95f);
        float newMultiplier = 1f - clampedPercent;

        m_SlowMultiplier = Mathf.Min(m_SlowMultiplier, newMultiplier);
        m_SlowTimeRemaining = Mathf.Max(m_SlowTimeRemaining, duration);
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // ONLY troops (BaseUnit). This ignores projectiles/tilemap/etc.
        BaseUnit troop = other.GetComponentInParent<BaseUnit>();
        if (troop == null) return;
        if (troop.IsDead) return;

        // start attacking only if not already attacking someone
        if (!isAttackingTroop)
        {
            contactTroop = troop;
            isAttackingTroop = true;
            damageCarry = 0f;
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (contactTroop == null) return;

        BaseUnit troop = other.GetComponentInParent<BaseUnit>();
        if (troop == contactTroop)
        {
            contactTroop = null;
            isAttackingTroop = false;
            damageCarry = 0f;
        }
    }

    protected virtual void OnDestroy()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
        }
    }
}
