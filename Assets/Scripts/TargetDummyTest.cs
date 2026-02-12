using UnityEngine;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    private bool registered = false;

    void Start()
    {
        currentHealth = maxHealth;

        // WaveManager controls spawn position (tilemap). Do NOT set position here.
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
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Always unregister even if destroyed by something else
        if (registered && WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
            registered = false;
        }
    }
}
