using UnityEngine;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f;
    public float spawnX = 12f;

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;

        // Spawn on the right
        transform.position = new Vector3(
            spawnX,
            transform.position.y,
            transform.position.z
        );
    }

    void Update()
    {
        // Move right → left
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
}
