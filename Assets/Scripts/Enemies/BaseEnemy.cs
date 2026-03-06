using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class BaseEnemy : MonoBehaviour
{
        // For polymorph support (ported from TargetDummyTest)
    private SpriteRenderer cachedRenderer;
    private int polymorphVersion;
    private Sprite baseSprite;
    public float HealthPercent => (maxHealth > 0) ? (float)currentHealth / maxHealth : 0f;
   
    // my take on wave scaling
    // Waves 1..START_AFTER_WAVE: base stats (no scaling)
    // After that, stats scale up once every SCALE_EVERY_N_WAVES.
    // Example: START_AFTER_WAVE = 3 and SCALE_EVERY_N_WAVES = 3
    // Waves 1-3: base stats
    // Waves 4-6: +10% once
    // Waves 7-9: +10% twice, etc.
    private const int START_AFTER_WAVE = 3;
    private const int SCALE_EVERY_N_WAVES = 3;   // change to 5 later if you want
    private const float SCALE_PER_STEP = 0.5f;  // 10% per step
    private const bool COMPOUND = true;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 0.5f; // how fast it moves left
    protected float m_SlowMultiplier = 1f;
    protected float m_SlowTimeRemaining;

    [Header("Health")]
    [SerializeField] protected int maxHealth = 50;
    public int currentHealth;

    [Header("Melee vs Troops")]
    [SerializeField] protected float damagePerSecond = 5f;

    protected BaseUnit contactTroop;
    protected bool isAttackingTroop;

    protected bool alreadyCountedAsEscape = false; // prevents double life loss

    protected Rigidbody2D rb;
    protected Collider2D enemyCollider;
    private BoardManager m_Board;
    public enum DebuffType
{
    Poison,
    Slow,
    Burn,
    Stun,
    DamageAmp
}
    public Dictionary<DebuffType, Debuff> Debuffs = new Dictionary<DebuffType, Debuff>();
    public float DamageAmp=1;
private float debuffTickTimer = 0f;
private const float DEBUFF_TICK_RATE = 1f;
    // damage accumulator so float DPS works with int HP
    private float damageCarry = 0f;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
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
        m_Board = FindFirstObjectByType<BoardManager>();
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

    protected virtual void Update()
    {

    debuffTickTimer += Time.deltaTime;
    if (debuffTickTimer >= DEBUFF_TICK_RATE)
    {
        DamageAmp=1;
        debuffTickTimer -= DEBUFF_TICK_RATE; // keeps it stable
        activateDebuff();
    }
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
            float enemyLeftX = enemyCollider != null ? enemyCollider.bounds.min.x : transform.position.x;
            if (enemyLeftX <= loseX)
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

    public virtual void TakeDamage(BaseUnit unit, int damage)
    {
        HitTint hitTint = GetComponent<HitTint>();
        if (hitTint != null)
        {
            hitTint.Flash();
        }
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            if (unit != null)
            {
                unit.OnKill();
            }
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
        HitTint hitTint = GetComponent<HitTint>();
        if (hitTint != null)
        {
            hitTint.Flash();
        }
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // ONLY troops (BaseUnit). This ignores projectiles/tilemap/etc.
        BaseUnit troop = other.GetComponentInParent<BaseUnit>();
        if (troop == null) return;
        if (troop.IsDead) return;
        if (!IsSameLane(troop.transform.position)) return;

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

    private bool IsSameLane(Vector3 troopWorldPos)
    {
        if (m_Board == null || m_Board.GameTilemap == null)
        {
            return Mathf.Abs(transform.position.y - troopWorldPos.y) < 0.05f;
        }

        Vector3Int enemyCell = m_Board.GameTilemap.WorldToCell(transform.position);
        Vector3Int troopCell = m_Board.GameTilemap.WorldToCell(troopWorldPos);
        return enemyCell.y == troopCell.y;
    }

    protected virtual void OnDestroy()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
        }
    }
public void ApplyDebuff(DebuffType type, float stacksToAdd, float duration, Action<float> func)
{
    //Refresh duration and add stack if already on
    if (Debuffs.ContainsKey(type))
    {
        Debuffs[type].amountOfStacks += stacksToAdd;
        Debuffs[type].duration = duration; 
    }
    else
    {
        // Apply new debuff
        Debuffs[type] = new Debuff(stacksToAdd, func, duration);
    }
}
public void activateDebuff()
    {
 foreach (KeyValuePair<DebuffType, Debuff> entry in Debuffs)
{

    Debuff debuff = entry.Value;

     debuff.func?.Invoke(debuff.amountOfStacks);
}
List<DebuffType> toRemove = new List<DebuffType>();

foreach (var kvp in Debuffs)
{
    kvp.Value.duration -= Time.deltaTime;

    if (kvp.Value.duration <= 0)
        toRemove.Add(kvp.Key);
}

foreach (var key in toRemove)
{
    Debuffs.Remove(key);
}
    }
    public void ApplyFire(float stacks)
{
      int damage = Mathf.RoundToInt(stacks);
      Debug.Log("enemy takes fire Damage");
    TakeDamage(null, damage);
    HitTint hitTint = GetComponent<HitTint>();
    if (hitTint != null)
    {
        hitTint.Flash();
    }
}
public void ApplyAmp(float stacks)
{
   
    Debug.Log("Amp Applied"+DamageAmp+" + "+stacks);
    DamageAmp+=stacks;
}
    
}
