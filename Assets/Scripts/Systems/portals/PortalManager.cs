using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PortalManager : MonoBehaviour
{
    [Header("References")]
    public BoardManager BoardManager;

    [Header("Portal Visuals")]
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

        SpriteRenderer renderer = GetOrCreatePortalRenderer(index);
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
        Debug.Log($"[PortalManager] Visual active for index {index}. Tier={portal.tier}, Cell=({portal.x}, {portal.y}), World={worldPosition}, Sprite={(sprite != null ? sprite.name : "NULL")}.");
    }

    private SpriteRenderer GetOrCreatePortalRenderer(int index)
    {
        if (m_PortalRenderers.TryGetValue(index, out SpriteRenderer existingRenderer) && existingRenderer != null)
        {
            return existingRenderer;
        }

        GameObject visualObject = new GameObject($"Portal_{index}", typeof(SpriteRenderer));
        visualObject.transform.SetParent(m_PortalVisualRoot, false);
        SpriteRenderer renderer = visualObject.GetComponent<SpriteRenderer>();
        m_PortalRenderers[index] = renderer;
        Debug.Log($"[PortalManager] Created renderer object {visualObject.name}.");
        return renderer;
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

    private Vector3 GetPortalWorldPosition(Portal portal)
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
