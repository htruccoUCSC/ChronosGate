// TargetDummyTest.cs
using UnityEngine;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed = 0.5f; // how fast it moves left
    private float m_SlowMultiplier = 1f;
    private float m_SlowTimeRemaining;

    [Header("Health")]
    private int maxHealth = 5000; // set health here
    private int currentHealth;

    [Header("Melee vs Troops")]
    [SerializeField] private int damagePerSecond = 5;

    private BaseUnit contactTroop;
    private bool isAttackingTroop;
    private float damageTickTimer;

    private bool registered = false;
    private bool alreadyCountedAsEscape = false; // prevents double life loss

    void Start()
    {
        currentHealth = maxHealth;

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
        if (!isAttackingTroop)
        {
            transform.position += Vector3.left * moveSpeed * m_SlowMultiplier * Time.deltaTime;
        }
        else
        {
            // tick damage
            if (contactTroop == null || contactTroop.IsDead)
            {
                contactTroop = null;
                isAttackingTroop = false;
            }
            else
            {
                damageTickTimer += Time.deltaTime;
                if (damageTickTimer >= 1f)
                {
                    damageTickTimer -= 1f;
                    contactTroop.TakeDamage(damagePerSecond);
                }
            }
        }

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
            if (transform.position.x <= loseX)
            {
                alreadyCountedAsEscape = true;
                WaveManager.Instance.EnemyReachedEnd(this);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        float clampedPercent = Mathf.Clamp(slowPercent, 0f, 0.95f);
        float newMultiplier = 1f - clampedPercent;

        m_SlowMultiplier = Mathf.Min(m_SlowMultiplier, newMultiplier);
        m_SlowTimeRemaining = Mathf.Max(m_SlowTimeRemaining, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseUnit troop = other.GetComponentInParent<BaseUnit>();
        if (troop == null) return; // ignore projectiles, tilemap, etc.

        Debug.Log("TRIGGER HIT TROOP: " + other.name);

        if (isAttackingTroop) return;
        if (troop.IsDead) return;

        contactTroop = troop;
        isAttackingTroop = true;
        damageTickTimer = 0f;
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (contactTroop == null) return;

        BaseUnit troop = other.GetComponentInParent<BaseUnit>();
        if (troop == contactTroop)
        {
            contactTroop = null;
            isAttackingTroop = false;
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
}
