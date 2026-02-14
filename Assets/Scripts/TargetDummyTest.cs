using UnityEngine;

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

    private void OnDestroy()
    {
        if (registered && WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
            registered = false;
        }
    }
}
