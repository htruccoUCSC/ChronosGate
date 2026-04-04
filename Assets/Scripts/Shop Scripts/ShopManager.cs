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
    
    [Header("Shop Slots")]
    [SerializeField] private ConsumableSlot[] consumableSlots = new ConsumableSlot[2];
    [SerializeField] private TowerSlot[] towerSlots = new TowerSlot[6];

    [Header("Data Sources")]
    [SerializeField] private DatabaseLoader databaseLoader;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private ItemInventoryUI itemInventoryUI;
    
    [Header("Tooltip")]
    [SerializeField] private GameObject consumableTooltip;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    [Header("Item Definition Source")]
    [SerializeField] private ItemDefinition[] availableItems;
    
    [Header("Progression Settings")]
    [SerializeField] private bool useProgressionFiltering = true;
    
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

        if (consumableTooltip != null)
        {
            consumableTooltip.SetActive(false);
        }

        UpdateShopUIState();
        
        // Initialize all consumable slots with reference to this manager
        foreach (ConsumableSlot slot in consumableSlots)
        {
            if (slot != null)
            {
                slot.Initialize(this);
            }
        }

        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (itemInventoryUI == null)
        {
            itemInventoryUI = FindFirstObjectByType<ItemInventoryUI>();
        }

        // Initialize consumable slots with item inventory UI
        foreach (ConsumableSlot slot in consumableSlots)
        {
            if (slot != null && itemInventoryUI != null)
            {
                slot.InitializeInventory(itemInventoryUI);
            }
        }

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
        
        PopulateConsumableSlots();
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

        if (!isShopOpen)
        {
            HideConsumableTooltip();
        }
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
        PopulateConsumableSlots();
        PopulateTowerSlots();
    }

    public void CloseShopPanel()
    {
        isShopOpen = false;
        HideConsumableTooltip();
        UpdateShopUIState();
    }

    public void SetGameplayUIVisible(bool visible)
    {
        bool wasGameplayUIVisible = gameplayUIVisible;
        gameplayUIVisible = visible;
        if (!visible)
        {
            isShopOpen = false;
            HideConsumableTooltip();
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
        rerollCost += 2; // Increase cost by 2 for next reroll
        UpdateRerollCostDisplay();
        PopulateConsumableSlots();
        PopulateTowerSlots();
    }
    
    private void UpdateRerollCostDisplay()
    {
        if (rerollCostText != null)
        {
            rerollCostText.text = $"Reroll: {rerollCost}";
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

    private void PopulateConsumableSlots()
    {
        if (consumableSlots == null || consumableSlots.Length == 0)
        {
            return;
        }

        if (availableItems == null || availableItems.Length == 0)
        {
            Debug.LogWarning("No available items assigned to populate consumable slots.");
            for (int i = 0; i < consumableSlots.Length; i++)
            {
                if (consumableSlots[i] != null)
                {
                    consumableSlots[i].Setup(null);
                }
            }
            return;
        }

        for (int i = 0; i < consumableSlots.Length; i++)
        {
            if (consumableSlots[i] == null)
            {
                continue;
            }

            ItemDefinition randomItem = availableItems[Random.Range(0, availableItems.Length)];
            consumableSlots[i].Setup(randomItem);
        }
    }

    // This method populates tower slots with random eligible UnitDefinitions from the database
    // We could consider chaning this to take a list of UnitDefinitions as a parameter if we want more control over what gets shown in the shop
    // For now, it will just pull all UnitDefinitions that have valid prefabs and randomly assign them to the tower slots so as we add units theyll show up in the shop without needing to update this code

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

        for (int i = 0; i < towerSlots.Length; i++)
        {
            if (towerSlots[i] == null)
            {
                continue;
            }

            UnitDefinition randomUnit = eligibleUnits[Random.Range(0, eligibleUnits.Count)];
            towerSlots[i].Setup(randomUnit);
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
    
    public void ShowConsumableTooltip(string description)
    {
        Debug.Log($"ShowConsumableTooltip called with: {description}");
        
        if (consumableTooltip == null)
        {
            Debug.LogError("consumableTooltip is null!");
            return;
        }
        
        if (tooltipText == null)
        {
            Debug.LogError("tooltipText is null!");
            return;
        }
        
        tooltipText.text = description;
        consumableTooltip.SetActive(true);
        Debug.Log("Tooltip should now be visible");
    }
    
    public void HideConsumableTooltip()
    {
        Debug.Log("HideConsumableTooltip called");
        
        if (consumableTooltip != null)
        {
            consumableTooltip.SetActive(false);
        }
    }
    
    [ContextMenu("Auto-Assign Slots")]
    private void AutoAssignSlots()
    {
        ConsumableSlot[] foundConsumables = shopPanel.GetComponentsInChildren<ConsumableSlot>();
        if (foundConsumables.Length > 0)
        {
            consumableSlots = foundConsumables;
            Debug.Log($"Assigned {consumableSlots.Length} consumable slots");
        }
        
        TowerSlot[] foundTowers = shopPanel.GetComponentsInChildren<TowerSlot>();
        if (foundTowers.Length > 0)
        {
            towerSlots = foundTowers;
            Debug.Log($"Assigned {towerSlots.Length} tower slots");
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    public void OnUnitPurchased(UnitDefinition unitDef)
    {
        // Try to get currency manager if not already cached
        if (currencyManager == null)
        {
            currencyManager = CurrencyManager.Instance;
        }
        
        if (currencyManager == null)
        {
            Debug.LogError("[ShopManager] CurrencyManager not found!");
            return;
        }
        
        // Check if player has enough currency
        if (!currencyManager.TrySpendCurrency(unitDef.Cost))
        {
            Debug.Log($"Not enough currency! Need {unitDef.Cost}, have {currencyManager.GetCurrency()}");
            // Show error message to player
            return;
        }
        
        // Unit purchased successfully
        Debug.Log($"Purchased {unitDef.Name} for {unitDef.Cost} gold!");
        // Proceed with adding unit to inventory
    }
}
