using UnityEngine;

// Place one instance of this in the scene (e.g. on the GameManager or a dedicated
// spawner GameObject). Assign the gold coin prefab in the Inspector.
// Units call CoinDropSpawner.Spawn(worldPos, amount) to drop a clickable coin.
public class CoinDropSpawner : MonoBehaviour
{
    public static CoinDropSpawner Instance { get; private set; }

    [SerializeField, Tooltip("The gold coin prefab — must have CoinDropVfx, SpriteRenderer, Animator, and Collider2D.")]
    private GameObject m_CoinPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Instantiates a coin at worldPos with the given gold value and launches it.
    // Falls back silently if the prefab is not assigned.
    public static void Spawn(Vector3 worldPos, int goldValue, float holdDuration = 30f)
    {
        if (Instance == null || Instance.m_CoinPrefab == null) return;

        GameObject coin = Instantiate(Instance.m_CoinPrefab, worldPos, Quaternion.identity);
        //set scale of coin to 1/4
        coin.transform.localScale = Instance.m_CoinPrefab.transform.localScale / 4f;
        CoinDropVfx vfx = coin.GetComponent<CoinDropVfx>();
        if (vfx == null) return;

        vfx.SetGoldValue(goldValue);
        vfx.SetHoldDuration(holdDuration);
        vfx.Launch();
    }
}
