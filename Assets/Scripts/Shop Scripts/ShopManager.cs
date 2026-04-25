using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button nextRoundButton;
    [SerializeField] private TextMeshProUGUI rerollCostText;
    [SerializeField] private TextMeshProUGUI toggleButtonLabel;
    [SerializeField] private string openShopText = "Open Shop";
    [SerializeField] private string closeShopText = "Close Shop";
    [SerializeField] private string pauseText = "Pause";
    [SerializeField] private string resumeText = "Resume";
    [SerializeField] private Color openShopButtonColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    [SerializeField] private Color closeShopButtonColor = new Color(0.85f, 0.25f, 0.25f, 1f);
    
    [Header("Tower Slot Spawning")]
    [SerializeField] private Transform m_TowerSlotContainer;
    [SerializeField] private GameObject m_TowerSlotPrefab;
    [SerializeField] private int m_TowerSlotCount = 6;
    [SerializeField] private Vector2 m_SlotCellSize = new Vector2(120f, 160f);
    [SerializeField] private Vector2 m_SlotSpacing = new Vector2(100f, 0f);

    private TowerSlot[] towerSlots;

    [Header("Data Sources")]
    [SerializeField] private DatabaseLoader databaseLoader;
    [SerializeField] private InventoryUI inventoryUI;
    
    [Header("Progression Settings")]
    [SerializeField] private bool useProgressionFiltering = true;

    [Header("Tower Pool")]
    [SerializeField] private int commonCopiesPerTowerInPool = 9;
    [SerializeField] private int uncommonCopiesPerTowerInPool = 9;
    [SerializeField] private int rareCopiesPerTowerInPool = 9;
    [SerializeField] private int epicCopiesPerTowerInPool = 9;
    [SerializeField] private int legendaryCopiesPerTowerInPool = 9;

    [Header("Debug")]
    [SerializeField] private bool logTowerRarityRolls = false;
    
    [Header("Visibility")]
    [SerializeField] private bool alwaysVisibleDuringGameplay = false;
    [SerializeField] private bool startOpenWhenGameplayVisible = true;

    private bool isShopOpen = false;
    private int rerollCost = 1;
    private CurrencyManager currencyManager;
    private UnitUnlockManager unlockManager;
    private BoardManager boardManager;
    private TextMeshProUGUI nextRoundButtonLabel;
    private string cachedNextRoundLabelText;
    private bool gameplayUIVisible;
    private readonly Dictionary<string, int> remainingTowerPoolCounts = new Dictionary<string, int>();
    private static readonly UnitRarity[] TowerRarityOrder =
    {
        UnitRarity.Common,
        UnitRarity.Uncommon,
        UnitRarity.Rare,
        UnitRarity.Epic,
        UnitRarity.Legendary
    };
    private static readonly float[,] TowerRarityOddsByRound =
    {
        { 60f, 30f, 7f, 2f, 1f },
        { 50f, 35f, 11f, 3f, 1f },
        { 39f, 40f, 15f, 5f, 1f },
        { 27f, 35f, 25f, 10f, 3f },
        { 20f, 30f, 30f, 15f, 5f },
        { 12f, 20f, 40f, 20f, 8f },
        { 10f, 15f, 30f, 30f, 15f },
        { 5f, 10f, 20f, 40f, 25f }
    };
    
    private void Start()
    {
        currencyManager = CurrencyManager.Instance;
        if (currencyManager == null)
        {
            Debug.LogWarning("[ShopManager] CurrencyManager not found at Start! Will try again when needed.");
        }
        else
        {
            currencyManager.OnCurrencyChanged += UpdateRerollButtonState;
        }
        
        unlockManager = UnitUnlockManager.Instance;
        boardManager = FindFirstObjectByType<BoardManager>();
        if (unlockManager == null && useProgressionFiltering)
        {
            Debug.LogWarning("[ShopManager] UnitUnlockManager not found! Progression filtering will be disabled.");
            useProgressionFiltering = false;
        }
        
        if (toggleButton == null)
        {
            toggleButton = FindButtonByName("toggleshopbutton");
        }

        if (toggleButton != null)
        {
            toggleButton.gameObject.SetActive(false);
        }

        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.AddListener(OnPauseButtonClicked);
        }
        else
        {
            Debug.LogWarning("[ShopManager] Pause button reference is missing.");
        }

        if (rerollButton != null)
        {
            rerollButton.onClick.AddListener(OnRerollButtonClicked);
        }
        else
        {
            Debug.LogWarning("[ShopManager] Reroll button reference is missing.");
        }
        CacheNextRoundButtonLabel();

        CacheToggleButtonLabel();
        
        gameplayUIVisible = false;
        isShopOpen = false;
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        BuildTowerSlots();

        UpdateShopUIState();

        foreach (TowerSlot slot in towerSlots)
        {
            if (slot != null)
            {
                slot.Initialize(inventoryUI);
            }
        }

        if (databaseLoader == null)
        {
            databaseLoader = FindFirstObjectByType<DatabaseLoader>();
        }

        UpdateRerollCostDisplay();
        
        // Wait for progression system to initialize before populating (WebGL compatibility)
        StartCoroutine(WaitAndPopulateShop());
    }

    private void BuildTowerSlots()
    {
        if (m_TowerSlotContainer == null || m_TowerSlotPrefab == null)
        {
            Debug.LogWarning("[ShopManager] TowerSlotContainer or TowerSlotPrefab not assigned. Cannot build tower slots.");
            towerSlots = new TowerSlot[0];
            return;
        }

        foreach (Transform child in m_TowerSlotContainer)
            Destroy(child.gameObject);

        GridLayoutGroup grid = m_TowerSlotContainer.GetComponent<GridLayoutGroup>();
        if (grid == null)
            grid = m_TowerSlotContainer.gameObject.AddComponent<GridLayoutGroup>();

        grid.cellSize = m_SlotCellSize;
        grid.spacing = m_SlotSpacing;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;

        towerSlots = new TowerSlot[m_TowerSlotCount];
        for (int i = 0; i < m_TowerSlotCount; i++)
        {
            GameObject slotObject = Instantiate(m_TowerSlotPrefab, m_TowerSlotContainer);
            slotObject.name = $"TowerSlot_{i}";
            TowerSlot slot = slotObject.GetComponent<TowerSlot>();
            if (slot == null)
                Debug.LogWarning($"[ShopManager] TowerSlotPrefab is missing a TowerSlot component on slot {i}.");
            towerSlots[i] = slot;
        }
    }

    private void Update()
    {
    }
    
    /// <summary>
    /// Waits for UnitUnlockManager to finish initializing, then populates shop.
    /// Required for WebGL async JSON loading.
    /// </summary>
    private IEnumerator WaitAndPopulateShop()
    {
        if (databaseLoader == null)
        {
            databaseLoader = FindFirstObjectByType<DatabaseLoader>();
        }

        if (databaseLoader != null)
        {
            while (!databaseLoader.IsLoaded)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("[ShopManager] DatabaseLoader not found. Tower slots may not populate.");
        }

        // Wait for unlock manager to be ready
        if (useProgressionFiltering && unlockManager != null)
        {
            while (!unlockManager.IsReady())
            {
                yield return null;
            }
            Debug.Log("[ShopManager] UnitUnlockManager ready, populating shop...");
        }
        
        PopulateTowerSlots();
    }
    
    public void ToggleShop()
    {
        if (!gameplayUIVisible)
        {
            return;
        }

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayUiClick();
        }

        if (alwaysVisibleDuringGameplay)
        {
            isShopOpen = true;
            UpdateShopUIState();
            return;
        }

        isShopOpen = !isShopOpen;
        UpdateShopUIState();
    }

    // method for gameloopmanger to open a new shop at the start of each round - also rerolls the shop to show new options
    public void OpenShop()
    {
        if (!gameplayUIVisible)
        {
            return;
        }

        isShopOpen = true;
        UpdateShopUIState();

        RefreshShopContents();
    }

    public void RefreshShopContents()
    {
        PopulateTowerSlots();
    }

    public void CloseShopPanel()
    {
        isShopOpen = false;
        UpdateShopUIState();
    }

    public void SetGameplayUIVisible(bool visible)
    {
        bool wasGameplayUIVisible = gameplayUIVisible;
        gameplayUIVisible = visible;
        if (!visible)
        {
            isShopOpen = false;
        }
        else if (alwaysVisibleDuringGameplay)
        {
            isShopOpen = true;
        }
        else if (!wasGameplayUIVisible && startOpenWhenGameplayVisible)
        {
            isShopOpen = true;
        }

        UpdateShopUIState();
        UpdatePauseButtonLabel();
    }

    private void UpdateShopUIState()
    {
        bool showPersistentShop = gameplayUIVisible && alwaysVisibleDuringGameplay;
        bool showPanel = gameplayUIVisible && (showPersistentShop || isShopOpen);
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(showPanel);
        }

        if (nextRoundButton != null)
        {
            bool showNextRound = gameplayUIVisible;
            nextRoundButton.gameObject.SetActive(showNextRound);
            nextRoundButton.interactable = showNextRound;
        }
    }
    
    private void CacheToggleButtonLabel()
    {
        if (toggleButtonLabel != null || toggleButton == null)
        {
            return;
        }

        toggleButtonLabel = toggleButton.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private Button FindButtonByName(string targetNameLower)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (Button candidate in buttons)
        {
            if (candidate == null)
            {
                continue;
            }

            string candidateName = candidate.gameObject.name.ToLower();
            if (candidateName.Contains(targetNameLower))
            {
                return candidate;
            }
        }

        return null;
    }

    private void UpdateToggleButtonLabel()
    {
        bool showingOpenState = !isShopOpen;

        if (toggleButtonLabel != null)
        {
            toggleButtonLabel.text = showingOpenState ? openShopText : closeShopText;
        }

        if (toggleButton != null && toggleButton.targetGraphic is Graphic graphic)
        {
            graphic.color = showingOpenState ? openShopButtonColor : closeShopButtonColor;
        }
    }

    private void UpdatePauseButtonLabel()
    {
        if (nextRoundButtonLabel == null)
        {
            return;
        }

        bool isPaused = GameSpeedButton.Instance != null && GameSpeedButton.Instance.IsPaused();
        nextRoundButtonLabel.text = isPaused ? resumeText : pauseText;
    }

    private void OnPauseButtonClicked()
    {
        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayUiClick();
        }

        if (GameSpeedButton.Instance != null)
        {
            GameSpeedButton.Instance.TogglePaused();
        }

        UpdatePauseButtonLabel();
    }

    private void CacheNextRoundButtonLabel()
    {
        if (nextRoundButton == null)
        {
            return;
        }

        nextRoundButtonLabel = nextRoundButton.GetComponentInChildren<TextMeshProUGUI>(true);
        if (nextRoundButtonLabel != null)
        {
            cachedNextRoundLabelText = nextRoundButtonLabel.text;
        }
        UpdatePauseButtonLabel();
    }
    
    private void OnRerollButtonClicked()
    {
        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayUiClick();
        }

        // Try to get currency manager if not already cached
        if (currencyManager == null)
        {
            currencyManager = CurrencyManager.Instance;
        }
        
        if (currencyManager == null)
        {
            Debug.LogError("[ShopManager] CurrencyManager not found! Make sure CurrencyManager exists in the scene and has initialized.");
            return;
        }

        if (!currencyManager.TrySpendCurrency(rerollCost))
        {
            Debug.Log($"[ShopManager] Cannot afford reroll! Need {rerollCost}, have {currencyManager.GetCurrency()}");
            return;
        }

        Debug.Log($"[ShopManager] Rerolled shop for {rerollCost} gold!");
        rerollCost += 2;
        UpdateRerollCostDisplay();

        PopulateTowerSlots();
    }
    
    private void UpdateRerollCostDisplay()
    {
        if (rerollCostText != null)
        {
            rerollCostText.text = $"{rerollCost}";
        }
    }
    
    private void UpdateRerollButtonState(int currentCurrency)
    {
        if (rerollButton != null)
        {
            rerollButton.interactable = currentCurrency >= rerollCost;
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= UpdateRerollButtonState;
        }
    }
    
    public void ResetRerollCost()
    {
        rerollCost = 1;
        UpdateRerollCostDisplay();
    }
    
    private void RerollShop()
    {
        PopulateTowerSlots();
    }

    private void PopulateTowerSlots()
    {
        if (towerSlots == null || towerSlots.Length == 0)
        {
            return;
        }

        if (databaseLoader == null)
        {
            Debug.LogWarning("DatabaseLoader not assigned. Cannot populate tower slots.");
            for (int i = 0; i < towerSlots.Length; i++)
            {
                if (towerSlots[i] != null)
                {
                    towerSlots[i].Setup(null);
                }
            }
            return;
        }

        var eligibleUnits = new List<UnitDefinition>();
        var fallbackUnits = new List<UnitDefinition>();

        // Get unlocked unit IDs from progression system
        List<string> unlockedUnitIDs = null;
        if (useProgressionFiltering && unlockManager != null)
        {
            unlockedUnitIDs = unlockManager.GetUnlockedUnitIDs();
            Debug.Log($"[ShopManager] Filtering shop by {(unlockedUnitIDs != null ? unlockedUnitIDs.Count.ToString() : "all")} unlocked units.");
        }

        foreach (var unitDef in databaseLoader.UnitLookup.Values)
        {
            if (unitDef == null || string.IsNullOrWhiteSpace(unitDef.UnitID))
            {
                continue;
            }

            // Filter by unlocked status if progression mode is enabled
            if (useProgressionFiltering && unlockedUnitIDs != null)
            {
                if (!unlockedUnitIDs.Contains(unitDef.UnitID))
                {
                    continue; // Skip locked units
                }
            }

            fallbackUnits.Add(unitDef);

            if (HasValidPrefab(unitDef))
            {
                eligibleUnits.Add(unitDef);
            }
        }

        if (eligibleUnits.Count == 0 && fallbackUnits.Count > 0)
        {
            Debug.LogWarning("[ShopManager] No units with valid prefabs found. Falling back to raw unit data from spreadsheet.");
            eligibleUnits.AddRange(fallbackUnits);
        }

        if (eligibleUnits.Count == 0)
        {
            Debug.LogWarning("No eligible UnitDefinitions with prefabs found to populate tower slots.");
            for (int i = 0; i < towerSlots.Length; i++)
            {
                if (towerSlots[i] != null)
                {
                    towerSlots[i].Setup(null);
                }
            }
            return;
        }

        // We keep a persistent pool for the whole run, then build temporary draw buckets from what's left.
        Dictionary<UnitRarity, List<UnitDefinition>> rarityPools = CreateEmptyRarityPools();

        for (int i = 0; i < eligibleUnits.Count; i++)
        {
            AddUnitToRarityPool(eligibleUnits[i], rarityPools);
        }

        for (int i = 0; i < towerSlots.Length; i++)
        {
            if (towerSlots[i] == null)
            {
                continue;
            }

            UnitRarity rolledRarity;
            List<UnitDefinition> selectedPool = GetRandomTowerRarityPool(rarityPools, out rolledRarity);
            if (selectedPool == null || selectedPool.Count == 0)
            {
                towerSlots[i].Setup(null);
                continue;
            }

            UnitDefinition randomUnit = selectedPool[Random.Range(0, selectedPool.Count)];
            towerSlots[i].Setup(randomUnit);
            selectedPool.Remove(randomUnit);

            if (logTowerRarityRolls)
            {
                Debug.Log($"[ShopManager] Wave {GetCurrentShopRound()} slot {i + 1}: rolled {rolledRarity}, selected {randomUnit.UnitID} ({randomUnit.Name}).");
            }
        }
    }

    private static bool HasValidPrefab(UnitDefinition unitDef)
    {
        if (unitDef == null || string.IsNullOrWhiteSpace(unitDef.PrefabPath))
        {
            return false;
        }

        return Resources.Load<GameObject>(unitDef.PrefabPath) != null;
    }

    private static Dictionary<UnitRarity, List<UnitDefinition>> CreateEmptyRarityPools()
    {
        var rarityPools = new Dictionary<UnitRarity, List<UnitDefinition>>(TowerRarityOrder.Length);
        for (int i = 0; i < TowerRarityOrder.Length; i++)
        {
            rarityPools[TowerRarityOrder[i]] = new List<UnitDefinition>();
        }

        return rarityPools;
    }

    private void AddUnitToRarityPool(UnitDefinition unitDef, Dictionary<UnitRarity, List<UnitDefinition>> rarityPools)
    {
        if (unitDef == null || !rarityPools.TryGetValue(unitDef.Rarity, out List<UnitDefinition> targetPool))
        {
            return;
        }

        int remainingCopies = GetRemainingTowerPoolCount(unitDef);
        for (int i = 0; i < remainingCopies; i++)
        {
            targetPool.Add(unitDef);
        }
    }

    private int GetRemainingTowerPoolCount(UnitDefinition unitDef)
    {
        if (unitDef == null || string.IsNullOrWhiteSpace(unitDef.UnitID))
        {
            return 0;
        }

        if (!remainingTowerPoolCounts.TryGetValue(unitDef.UnitID, out int remainingCopies))
        {
            remainingCopies = Mathf.Max(0, GetCopiesPerTowerInPool(unitDef.Rarity));
            remainingTowerPoolCounts[unitDef.UnitID] = remainingCopies;
        }

        return remainingCopies;
    }

    private int GetCopiesPerTowerInPool(UnitRarity rarity)
    {
        switch (rarity)
        {
            case UnitRarity.Uncommon:
                return uncommonCopiesPerTowerInPool;
            case UnitRarity.Rare:
                return rareCopiesPerTowerInPool;
            case UnitRarity.Epic:
                return epicCopiesPerTowerInPool;
            case UnitRarity.Legendary:
                return legendaryCopiesPerTowerInPool;
            default:
                return commonCopiesPerTowerInPool;
        }
    }

    private void DecrementTowerPool(UnitDefinition unitDef)
    {
        if (unitDef == null || string.IsNullOrWhiteSpace(unitDef.UnitID))
        {
            return;
        }

        int remainingCopies = GetRemainingTowerPoolCount(unitDef);
        if (remainingCopies <= 0)
        {
            return;
        }

        remainingTowerPoolCounts[unitDef.UnitID] = remainingCopies - 1;
    }

    private List<UnitDefinition> GetRandomTowerRarityPool(Dictionary<UnitRarity, List<UnitDefinition>> rarityPools, out UnitRarity rolledRarity)
    {
        rolledRarity = UnitRarity.Common;
        int shopRound = GetCurrentShopRound();
        int oddsRowIndex = Mathf.Clamp(shopRound - 1, 0, TowerRarityOddsByRound.GetLength(0) - 1);
        float totalWeight = 0f;
        float[] enabledWeights = new float[TowerRarityOrder.Length];

        // If a rarity pool is empty it gets 0 weight, so we never roll into a dead pool.
        for (int i = 0; i < TowerRarityOrder.Length; i++)
        {
            UnitRarity rarity = TowerRarityOrder[i];
            if (!rarityPools.TryGetValue(rarity, out List<UnitDefinition> pool) || pool.Count == 0)
            {
                enabledWeights[i] = 0f;
                continue;
            }

            float weight = Mathf.Max(0f, TowerRarityOddsByRound[oddsRowIndex, i]);
            enabledWeights[i] = weight;
            totalWeight += weight;
        }

        if (totalWeight <= 0f)
        {
            // If every enabled rarity is empty, we want an empty shop slot, not a fallback into disabled rarities.
            return null;
        }

        float roll = Random.Range(0f, totalWeight);
        for (int i = 0; i < TowerRarityOrder.Length; i++)
        {
            float weight = enabledWeights[i];
            if (weight <= 0f)
            {
                continue;
            }

            if (roll < weight)
            {
                rolledRarity = TowerRarityOrder[i];
                return rarityPools[TowerRarityOrder[i]];
            }

            roll -= weight;
        }

        rolledRarity = TowerRarityOrder[TowerRarityOrder.Length - 1];
        return rarityPools[TowerRarityOrder[TowerRarityOrder.Length - 1]];
    }

    private int GetCurrentShopRound()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        GameLoopManager gameLoopManager = FindFirstObjectByType<GameLoopManager>();
        if (waveManager == null)
        {
            return 1;
        }

        int wavesPerCycle = gameLoopManager != null ? Mathf.Max(1, gameLoopManager.WavesPerAugmentCycle) : 1;
        return Mathf.Max(1, ((Mathf.Max(1, waveManager.currentWave) - 1) / wavesPerCycle) + 1);
    }
    
    public void OnUnitPurchased(UnitDefinition unitDef)
    {
        if (unitDef == null)
        {
            return;
        }

        DecrementTowerPool(unitDef);
        Debug.Log($"[ShopManager] Removed {unitDef.Name} from the remaining tower pool. Copies left: {GetRemainingTowerPoolCount(unitDef)}");
    }

    public void AddTowerPoolCopies(UnitDefinition unitDef, int amount)
    {
        if (unitDef == null || string.IsNullOrWhiteSpace(unitDef.UnitID))
        {
            return;
        }

        int safeAmount = Mathf.Max(0, amount);
        if (safeAmount <= 0)
        {
            return;
        }

        int remainingCopies = GetRemainingTowerPoolCount(unitDef);
        remainingTowerPoolCounts[unitDef.UnitID] = remainingCopies + safeAmount;
        Debug.Log($"[ShopManager] Added {safeAmount} copy to {unitDef.Name}. Copies left: {GetRemainingTowerPoolCount(unitDef)}");
    }
}
