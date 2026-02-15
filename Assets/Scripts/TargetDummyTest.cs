using UnityEngine;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f; // how fast it moves left

    [Header("Health")]
    public int maxHealth = 15;
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
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

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

    private void OnDestroy()
    {
        if (registered && WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
            registered = false;
        }
    }
}
