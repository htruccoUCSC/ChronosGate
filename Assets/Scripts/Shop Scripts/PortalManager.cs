using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages portal interactions and transitions between game states.
/// </summary>
public class PortalManager : MonoBehaviour
{
    private const int PortalCount = 5;
    private const int UnfilteredPortalIndex = 2;
    private const float FactionTintSaturation = 0.6f;
    private const float FactionTintValue = 1f;

    [Serializable]
    private struct PortalDefinition
    {
        [TextArea(2, 4)]
        public string Description;
        public Color Tint;
        public string Faction;
    }

    [Header("References")]
    [SerializeField] private GameLoopManager m_gameLoopManager;
    [SerializeField] private RectTransform m_portalContainer;
    [SerializeField] private GameObject m_portalSelectionRoot;
    [SerializeField] private PortalOption m_portalPrefab;
    [SerializeField] private DatabaseLoader m_databaseLoader;
    [SerializeField] private PortalDefinition[] m_portalDefinitions = new PortalDefinition[PortalCount];
    [SerializeField] private bool m_randomizeFactions = true;
    [SerializeField] private RectTransform[] m_spawnPoints;
    [SerializeField] private Vector2 m_startOffset;
    [SerializeField] private float m_spacing;

    private readonly List<PortalOption> m_spawnedPortals = new List<PortalOption>();
    private bool m_isOpen = false;
    private PortalDefinition[] m_activeDefinitions;

    private void Awake()
    {
        if (m_portalSelectionRoot != null)
        {
            m_portalSelectionRoot.SetActive(false);
        }
    }

    private void Start()
    {
        Debug.Log("[PortalManager] Start, ensuring portals are spawned.");
        RefreshPortalDefinitions();
        EnsurePortalsSpawned();
    }

    public void OpenSelection()
    {
        Debug.Log("[PortalManager] OpenSelection called.");
        RefreshPortalDefinitions();
        EnsurePortalsSpawned();
        ApplyDefinitionsToPortals();
        m_isOpen = true;

        if (m_portalSelectionRoot != null)
        {
            m_portalSelectionRoot.SetActive(true);
        }
        else if (m_portalContainer != null)
        {
            m_portalContainer.gameObject.SetActive(true);
        }

        foreach (PortalOption portal in m_spawnedPortals)
        {
            portal.SetInteractable(true);
        }
    }

    private void EnsurePortalsSpawned()
    {
        if (m_portalPrefab == null || m_portalContainer == null)
        {
            Debug.LogWarning("[PortalManager] Missing portalPrefab or portalContainer.");
            return;
        }

        if (m_spawnedPortals.Count > 0)
        {
            Debug.Log("[PortalManager] Portals already spawned.");
            return;
        }

        int portalCount = GetPortalCount();
        for (int i = 0; i < portalCount; i++)
        {
            PortalOption portal = Instantiate(m_portalPrefab, m_portalContainer);
            portal.name = $"PortalOption_{i + 1}";
            RectTransform portalRect = portal.GetComponent<RectTransform>();
            Vector2 spawnPosition = GetSpawnPosition(i, portalCount);
            if (portalRect != null)
            {
                portalRect.anchoredPosition = spawnPosition;
            }
            else
            {
                portal.transform.localPosition = spawnPosition;
            }

            PortalDefinition definition = GetDefinition(i);
            InitializePortal(portal, i, definition);
            m_spawnedPortals.Add(portal);
        }

        Debug.Log($"[PortalManager] Spawned {m_spawnedPortals.Count} portals under {m_portalContainer.name}.");
    }

    private PortalDefinition GetDefinition(int index)
    {
        if (m_activeDefinitions != null && index < m_activeDefinitions.Length)
        {
            return m_activeDefinitions[index];
        }

        if (m_portalDefinitions != null && index < m_portalDefinitions.Length)
        {
            return m_portalDefinitions[index];
        }

        return new PortalDefinition
        {
            Description = string.Empty,
            Tint = Color.white,
            Faction = null
        };
    }

    private Vector2 GetSpawnPosition(int index, int portalCount)
    {
        if (m_spawnPoints != null && index < m_spawnPoints.Length && m_spawnPoints[index] != null)
        {
            return m_spawnPoints[index].anchoredPosition;
        }

        float centeredIndex = index - (portalCount - 1) * 0.5f;
        return new Vector2(m_startOffset.x + centeredIndex * m_spacing, m_startOffset.y);
    }

    private int GetPortalCount()
    {
        if (m_activeDefinitions != null && m_activeDefinitions.Length > 0)
        {
            return m_activeDefinitions.Length;
        }

        if (m_portalDefinitions != null && m_portalDefinitions.Length > 0)
        {
            return m_portalDefinitions.Length;
        }

        return PortalCount;
    }

    private void HandlePortalClicked(int index)
    {
        if (!m_isOpen)
        {
            return;
        }

        m_isOpen = false;

        if (m_portalSelectionRoot != null)
        {
            m_portalSelectionRoot.SetActive(false);
        }
        else if (m_portalContainer != null)
        {
            m_portalContainer.gameObject.SetActive(false);
        }

        foreach (PortalOption portal in m_spawnedPortals)
        {
            portal.SetInteractable(false);
        }

        if (m_gameLoopManager == null)
        {
            m_gameLoopManager = GameLoopManager.Instance;
        }

        if (m_gameLoopManager != null)
        {
            PortalDefinition definition = GetDefinition(index);
            string faction = string.IsNullOrWhiteSpace(definition.Faction) ? null : definition.Faction;
            m_gameLoopManager.OnPortalSelected(index, faction);
        }
        else
        {
            Debug.LogWarning("[PortalManager] GameLoopManager missing, cannot advance to shop.");
        }
    }

    private void RefreshPortalDefinitions()
    {
        if (!m_randomizeFactions)
        {
            m_activeDefinitions = m_portalDefinitions;
            return;
        }

        List<string> factions = GetAvailableFactions();
        if (factions.Count == 0)
        {
            m_activeDefinitions = m_portalDefinitions;
            return;
        }

        ShuffleFactions(factions);
        int portalCount = PortalCount;
        m_activeDefinitions = new PortalDefinition[portalCount];

        for (int i = 0; i < portalCount; i++)
        {
            if (i == UnfilteredPortalIndex)
            {
                m_activeDefinitions[i] = new PortalDefinition
                {
                    Faction = null,
                    Description = "All Units",
                    Tint = Color.white
                };
                continue;
            }

            string faction = i < factions.Count
                ? factions[i]
                : factions[UnityEngine.Random.Range(0, factions.Count)];

            m_activeDefinitions[i] = new PortalDefinition
            {
                Faction = faction,
                Description = string.Format("{0}", faction),
                Tint = GetFactionTint(faction)
            };
        }
    }

    private List<string> GetAvailableFactions()
    {
        if (m_databaseLoader == null)
        {
            m_databaseLoader = FindFirstObjectByType<DatabaseLoader>();
        }

        var factions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (m_databaseLoader == null || m_databaseLoader.UnitLookup == null || m_databaseLoader.UnitLookup.Count == 0)
        {
            return new List<string>();
        }

        foreach (UnitDefinition unitDef in m_databaseLoader.UnitLookup.Values)
        {
            if (unitDef == null || string.IsNullOrWhiteSpace(unitDef.Faction))
            {
                continue;
            }

            factions.Add(unitDef.Faction.Trim());
        }

        return new List<string>(factions);
    }

    private static void ShuffleFactions(List<string> factions)
    {
        for (int i = 0; i < factions.Count; i++)
        {
            int swapIndex = UnityEngine.Random.Range(i, factions.Count);
            string temp = factions[i];
            factions[i] = factions[swapIndex];
            factions[swapIndex] = temp;
        }
    }

    private Color GetFactionTint(string faction)
    {
        if (string.IsNullOrWhiteSpace(faction))
        {
            return Color.white;
        }

        int hash = faction.GetHashCode();
        float hue = Mathf.Abs(hash % 360) / 360f;
        return Color.HSVToRGB(hue, FactionTintSaturation, FactionTintValue);
    }

    private void InitializePortal(PortalOption portal, int index, PortalDefinition definition)
    {
        Color tint = definition.Tint.a <= 0f ? Color.white : definition.Tint;
        portal.Initialize(index, definition.Description, tint, HandlePortalClicked);
    }

    private void ApplyDefinitionsToPortals()
    {
        int portalCount = Mathf.Min(m_spawnedPortals.Count, GetPortalCount());
        for (int i = 0; i < portalCount; i++)
        {
            PortalOption portal = m_spawnedPortals[i];
            if (portal == null)
            {
                continue;
            }

            PortalDefinition definition = GetDefinition(i);
            InitializePortal(portal, i, definition);
        }
    }
}