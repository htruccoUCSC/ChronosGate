using UnityEngine;
using System.Collections;

public class TargetDummyTest : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1f; // how fast it moves left

    [Header("Health")]
    public int maxHealth = 15;
    private int currentHealth;

    private bool registered = false;
    private bool alreadyCountedAsEscape = false; // prevents double life loss
    private int slowVersion = 0;
    private int polymorphVersion = 0;
    private float baseMoveSpeed;
    private SpriteRenderer cachedRenderer;
    private Sprite baseSprite;

    void Start()
    {
        currentHealth = maxHealth;
        baseMoveSpeed = moveSpeed;
        cachedRenderer = GetComponentInChildren<SpriteRenderer>();
        if (cachedRenderer != null)
        {
            baseSprite = cachedRenderer.sprite;
        }

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
        Debug.Log($"Target Dummy took {damage} damage, current health: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        float clampedSlow = Mathf.Clamp(slowPercent, 0f, 0.95f);
        float clampedDuration = Mathf.Max(0f, duration);
        int version = ++slowVersion;

        moveSpeed = baseMoveSpeed * (1f - clampedSlow);

        if (clampedDuration <= 0f)
        {
            return;
        }

        StartCoroutine(RemoveSlowAfterDuration(version, clampedDuration));
    }

    private IEnumerator RemoveSlowAfterDuration(int version, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (version == slowVersion)
        {
            moveSpeed = baseMoveSpeed;
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

    private void OnDestroy()
    {
        if (registered && WaveManager.Instance != null)
        {
            WaveManager.Instance.UnregisterEnemy(this);
            registered = false;
        }
    }
}
