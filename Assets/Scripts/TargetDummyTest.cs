using UnityEngine;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;   // how fast the enemy moves left

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    private bool registered = false;

    void Start()
    {
        currentHealth = maxHealth;

        // IMPORTANT:
        // Do NOT set transform.position here.
        // WaveManager spawns us at the correct position.

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
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Always unregister no matter how we get destroyed
        if (registered && WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
            registered = false;
        }
    }
}
