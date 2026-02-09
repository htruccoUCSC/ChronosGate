using UnityEngine;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement")]
<<<<<<< HEAD
    public float moveSpeed = 1f;
    public float spawnX = 12f;
=======
    public float moveSpeed = 1f;   // how fast the enemy moves left
>>>>>>> main

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;
<<<<<<< HEAD
=======

    private bool registered = false;
>>>>>>> main

    void Start()
    {
        currentHealth = maxHealth;

<<<<<<< HEAD
        // Spawn on the right
        transform.position = new Vector3(
            spawnX,
            transform.position.y,
            transform.position.z
        );
=======
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
>>>>>>> main
    }

    void Update()
    {
<<<<<<< HEAD
        // Move right → left
=======
>>>>>>> main
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
<<<<<<< HEAD
=======

    private void OnDestroy()
    {
        // Always unregister no matter how we get destroyed
        if (registered && WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
            registered = false;
        }
    }
>>>>>>> main
}
