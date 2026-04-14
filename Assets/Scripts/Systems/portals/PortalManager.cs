using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PortalManager : MonoBehaviour
{
    [Header("References")]
    public BoardManager BoardManager;

    [Header("Portal Visuals")]
    [SerializeField] private GameObject m_PortalVisualPrefab;
    [SerializeField] private Sprite m_Level1Sprite;
    [SerializeField] private Sprite m_Level2Sprite;
    [SerializeField] private Sprite m_Level3Sprite;
    [SerializeField] private Vector3 m_VisualOffset = new Vector3(0f, 0f, -0.25f);
    [SerializeField] private Vector3 m_VisualScale = new Vector3(3f, 3f, 1f);
    [SerializeField] private string m_SortingLayerName = "Default";
    [SerializeField] private int m_SortingOrder = 5000;

    private Portal[] m_PortalArray;
    private readonly List<int> m_ViableIndices = new List<int>();
    private readonly Dictionary<int, SpriteRenderer> m_PortalRenderers = new Dictionary<int, SpriteRenderer>();
    // Bayo Bandele - 4/11/2026: added to track each lane's Animator so pre-spawn and spawn clips can be triggered
    private readonly Dictionary<int, Animator> m_PortalAnimators = new Dictionary<int, Animator>();
    // Bayo Bandele - 4/13/2026: tracks when each portal's spawn animation finishes so pre-spawn won't overlap it
    private readonly Dictionary<int, float> m_PortalSpawnReadyTimes = new Dictionary<int, float>();
    private float m_SpawnClipLength = 0f;
    private Transform m_PortalVisualRoot;

    public void init()
    {
        Debug.Log("[PortalManager] init() called.");
        EnsureInitialized();
        OpenStartingPortal();
        RefreshPortalVisuals();
    }

    public void addPortal(int amount)
    {
        EnsureInitialized();

        for (int i = 0; i < amount; i++)
        {
            if (m_ViableIndices.Count == 0)
            {
                break;
            }

            bool appliedPortalChange = false;
            while (!appliedPortalChange && m_ViableIndices.Count > 0)
            {
                int viableListIndex = Random.Range(0, m_ViableIndices.Count);
                int selectedIndex = m_ViableIndices[viableListIndex];
                Portal portal = m_PortalArray[selectedIndex];

                if (portal == null)
                {
                    if (!IsSpawnablePortalIndex(selectedIndex))
                    {
                        m_ViableIndices.RemoveAt(viableListIndex);
                        continue;
                    }

                    m_PortalArray[selectedIndex] = new Portal(1f, GetPortalColumnX(), selectedIndex);
                    UpdatePortalVisual(selectedIndex);
                    appliedPortalChange = true;
                    continue;
                }

                if (portal.tier < 3f)
                {
                    portal.tier += 1f;
                    UpdatePortalVisual(selectedIndex);
                    appliedPortalChange = true;
                    if (portal.tier >= 3f)
                    {
                        m_ViableIndices.RemoveAt(viableListIndex);
                    }
                    continue;
                }

                m_ViableIndices.RemoveAt(viableListIndex);
            }
        }

        RebuildViableIndices();
        RefreshPortalVisuals();
    }

    // Bayo Bandele - 4/11/2026: trigger the pre-spawn warning animation on a portal before an enemy exits
    public void TriggerPortalPreSpawn(int index)
    {
        if (m_PortalAnimators.TryGetValue(index, out Animator anim) && anim != null)
        {
            anim.SetTrigger("PreSpawn");
        }
    }

    public Portal GetPortal(int index)
    {
        EnsureInitialized();

        if (m_PortalArray == null || index < 0 || index >= m_PortalArray.Length)
        {
            return null;
        }

        return m_PortalArray[index];
    }

    public IReadOnlyList<Portal> GetPortals()
    {
        EnsureInitialized();
        return m_PortalArray;
    }

    // Bayo Bandele - 4/11/2026: Gets the portal animators dictionary as read only.
        public IReadOnlyDictionary<int, Animator> GetPortalAnimators()
    {
        return m_PortalAnimators;
    }

    private void RefreshPortalVisuals()
    {
        EnsurePortalVisualRoot();

        if (m_PortalArray == null)
        {
            Debug.LogWarning("[PortalManager] RefreshPortalVisuals aborted: portal array is null.");
            return;
        }

        Debug.Log($"[PortalManager] Refreshing portal visuals for {m_PortalArray.Length} rows.");
        for (int i = 0; i < m_PortalArray.Length; i++)
        {
            UpdatePortalVisual(i);
        }
    }

    private void UpdatePortalVisual(int index)
    {
        EnsurePortalVisualRoot();

        if (m_PortalArray == null || index < 0 || index >= m_PortalArray.Length)
        {
            return;
        }

        Portal portal = m_PortalArray[index];
        if (portal == null)
        {
            Debug.Log($"[PortalManager] No portal at index {index}; hiding visual if it exists.");
            if (m_PortalRenderers.TryGetValue(index, out SpriteRenderer hiddenRenderer) && hiddenRenderer != null)
            {
                hiddenRenderer.gameObject.SetActive(false);
            }
            return;
        }

        bool isNewPortal = GetOrCreatePortalRenderer(index, out SpriteRenderer renderer, out Animator animator);
        Vector3 worldPosition = GetPortalWorldPosition(portal);
        Sprite sprite = GetSpriteForTier(portal.tier);

        if (sprite == null)
        {
            Debug.LogError($"[PortalManager] Missing sprite for tier {portal.tier} at index {index}. Renderer object was still created so this is visible in hierarchy.");
        }

        renderer.sprite = sprite;
        renderer.color = sprite != null ? Color.white : Color.magenta;
        renderer.transform.position = worldPosition;
        renderer.transform.localScale = m_VisualScale;
        renderer.transform.rotation = Quaternion.identity;
        renderer.gameObject.SetActive(true);
        renderer.sortingLayerName = m_SortingLayerName;
        renderer.sortingOrder = m_SortingOrder;

        // Bayo Bandele - 4/11/2026: play the spawn animation only when a portal is first created, not on refresh
        // Bayo Bandele - 4/13/2026: record when the spawn clip will finish so WaveManager can wait before triggering pre-spawn
        if (isNewPortal && animator != null)
        {
            CacheSpawnClipLength(animator);
            m_PortalSpawnReadyTimes[index] = Time.time + m_SpawnClipLength;
            animator.SetTrigger("Spawn");
        }

        Debug.Log($"[PortalManager] Visual active for index {index}. Tier={portal.tier}, Cell=({portal.x}, {portal.y}), World={worldPosition}, Sprite={(sprite != null ? sprite.name : "NULL")}.");
    }

    // Returns true if this is a newly created portal (so caller can trigger spawn animation).
    // Bayo Bandele - 4/11/2026: added out Animator parameter so the spawn clip can be triggered on new portals
    private bool GetOrCreatePortalRenderer(int index, out SpriteRenderer renderer, out Animator animator)
    {
        if (m_PortalRenderers.TryGetValue(index, out SpriteRenderer existingRenderer) && existingRenderer != null)
        {
            renderer = existingRenderer;
            m_PortalAnimators.TryGetValue(index, out animator);
            return false;
        }

        GameObject visualObject = m_PortalVisualPrefab != null
            ? Instantiate(m_PortalVisualPrefab)
            : new GameObject($"Portal_{index}", typeof(SpriteRenderer));

        visualObject.name = $"Portal_{index}";
        visualObject.transform.SetParent(m_PortalVisualRoot, false);

        renderer = visualObject.GetComponent<SpriteRenderer>() ?? visualObject.AddComponent<SpriteRenderer>();

        // Bayo Bandele - 4/11/2026: grab the Animator off the portal prefab for clip control
        animator = visualObject.GetComponent<Animator>();

        m_PortalRenderers[index] = renderer;
        // Bayo Bandele - 4/11/2026: cache the animator by lane index so TriggerPortalPreSpawn can look it up
        if (animator != null)
        {
            m_PortalAnimators[index] = animator;
        }

        Debug.Log($"[PortalManager] Created portal visual {visualObject.name} (prefab={m_PortalVisualPrefab != null}).");
        return true;
    }

    private Sprite GetSpriteForTier(float tier)
    {
        int roundedTier = Mathf.RoundToInt(tier);
        switch (roundedTier)
        {
            case 1:
                return m_Level1Sprite;
            case 2:
                return m_Level2Sprite;
            case 3:
                return m_Level3Sprite;
            default:
                return null;
        }
    }

    // Bayo Bandele - 4/13/2026: returns true once the portal spawn animation has finished, so pre-spawn won't overlap it
    public bool IsPortalSpawnComplete(int index)
    {
        return !m_PortalSpawnReadyTimes.ContainsKey(index) || Time.time >= m_PortalSpawnReadyTimes[index];
    }

    // Bayo Bandele - 4/13/2026: reads the Portal1Spawn clip length once and caches it so we don't walk the clip list every spawn
    private void CacheSpawnClipLength(Animator animator)
    {
        if (m_SpawnClipLength > 0f) return;
        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == "Portal1Spawn")
            {
                m_SpawnClipLength = clip.length;
                return;
            }
        }
    }

    // Bayo Bandele - 4/11/2026: added so WaveManager can get the portal's world position from one place instead of computing it inline
    public Vector3 GetPortalWorldPosition(Portal portal)
    {
        return new Vector3(portal.x, portal.y + 0.5f, 0f);
    }

    private void EnsurePortalVisualRoot()
    {
        if (m_PortalVisualRoot != null)
        {
            return;
        }

        GameObject existingRoot = GameObject.Find("PortalVisuals_Runtime");
        if (existingRoot != null)
        {
            m_PortalVisualRoot = existingRoot.transform;
            return;
        }

        GameObject rootObject = new GameObject("PortalVisuals_Runtime");
        m_PortalVisualRoot = rootObject.transform;
        m_PortalVisualRoot.SetParent(null, true);
    }

    private void EnsureInitialized()
    {
        if (BoardManager == null)
        {
            BoardManager = FindFirstObjectByType<BoardManager>();
            Debug.Log($"[PortalManager] BoardManager auto-found: {BoardManager != null}.");
        }

        int boardHeight = BoardManager != null ? Mathf.Max(0, BoardManager.Height) : 0;
        if (boardHeight <= 0)
        {
            Debug.LogWarning("[PortalManager] EnsureInitialized aborted: board height is 0.");
            return;
        }

        if (m_PortalArray == null || m_PortalArray.Length != boardHeight)
        {
            Portal[] previousPortals = m_PortalArray;
            m_PortalArray = new Portal[boardHeight];
            Debug.Log($"[PortalManager] Portal array initialized to height {boardHeight}.");

            if (previousPortals != null)
            {
                int copyLength = Mathf.Min(previousPortals.Length, m_PortalArray.Length);
                for (int i = 0; i < copyLength; i++)
                {
                    m_PortalArray[i] = previousPortals[i];
                }
            }
        }

        RebuildViableIndices();
    }

    private void OpenStartingPortal()
    {
        if (m_PortalArray == null)
        {
            Debug.LogWarning("[PortalManager] OpenStartingPortal aborted: portal array is null.");
            return;
        }

        for (int i = 0; i < m_PortalArray.Length; i++)
        {
            if (m_PortalArray[i] != null)
            {
                return;
            }
        }

        List<int> spawnableIndices = new List<int>();
        for (int i = 0; i < m_PortalArray.Length; i++)
        {
            if (IsSpawnablePortalIndex(i))
            {
                spawnableIndices.Add(i);
            }
        }

        if (spawnableIndices.Count == 0)
        {
            Debug.LogWarning("[PortalManager] No spawnable portal indices found.");
            return;
        }

        int selectedIndex = spawnableIndices[Random.Range(0, spawnableIndices.Count)];
        m_PortalArray[selectedIndex] = new Portal(1f, GetPortalColumnX(), selectedIndex);
        Debug.Log($"[PortalManager] Starting portal created at index {selectedIndex} with x={m_PortalArray[selectedIndex].x}, y={m_PortalArray[selectedIndex].y}.");
        RebuildViableIndices();
    }

    private void RebuildViableIndices()
    {
        m_ViableIndices.Clear();

        if (m_PortalArray == null)
        {
            return;
        }

        for (int i = 0; i < m_PortalArray.Length; i++)
        {
            if (m_PortalArray[i] == null || m_PortalArray[i].tier < 3f)
            {
                m_ViableIndices.Add(i);
            }
        }
    }

    private bool IsSpawnablePortalIndex(int index)
    {
        if (BoardManager == null)
        {
            return false;
        }

        return index > 0 && index < BoardManager.Height - 1;
    }

    private float GetPortalColumnX()
    {
        if (BoardManager == null)
        {
            return 0f;
        }

        return Mathf.Max(0, BoardManager.Width);
    }
}
