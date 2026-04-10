using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManagerOld : MonoBehaviour
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
    [SerializeField] private bool showTooltipOverlay = false;

    [Header("Item Definition Source")]
    [SerializeField] private ItemDefinition[] availableItems;

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

    [Header("Next Round Confirmation")]
    [SerializeField] private float noTowerConfirmWindowSeconds = 2f;
    [SerializeField] private string noTowerConfirmWarning = "No towers placed. Click Next Round again to confirm.";

    private bool isShopOpen;
    private int rerollCost = 1;
    private CurrencyManager currencyManager;
    private UnitUnlockManager unlockManager;
    private BoardManager boardManager;
    private bool awaitingNoTowerConfirmation;
    private float noTowerConfirmExpiresAt;
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
        unlockManager = UnitUnlockManager.Instance;
        boardManager = FindFirstObjectByType<BoardManager>();

        if (unlockManager == null && useProgressionFiltering)
        {
            Debug.LogWarning("[ShopManagerOld] UnitUnlockManager not found. Progression filtering disabled.");
            useProgressionFiltering = false;
        }

        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleShop);
            toggleButton.onClick.AddListener(ToggleShop);
        }

        if (nextRoundButton != null)
        {
            nextRoundButton.onClick.RemoveListener(OnNextRoundButtonClicked);
            nextRoundButton.onClick.AddListener(OnNextRoundButtonClicked);
        }

        if (rerollButton != null)
        {
            rerollButton.onClick.RemoveListener(OnRerollButtonClicked);
            rerollButton.onClick.AddListener(OnRerollButtonClicked);
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        if (consumableTooltip != null)
        {
            consumableTooltip.SetActive(false);
        }

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
        UpdateShopUIState();
        StartCoroutine(WaitAndPopulateShop());
    }

    private void Update()
    {
        if (awaitingNoTowerConfirmation && Time.unscaledTime > noTowerConfirmExpiresAt)
        {
            ClearNoTowerConfirmationState();
        }
    }

    private IEnumerator WaitAndPopulateShop()
    {
        if (databaseLoader != null)
        {
            while (!databaseLoader.IsLoaded)
            {
                yield return null;
            }
        }

        if (useProgressionFiltering && unlockManager != null)
        {
            while (!unlockManager.IsReady())
            {
                yield return null;
            }
        }

        PopulateConsumableSlots();
        PopulateTowerSlots();
    }

    public void ToggleShop()
    {
        ClearNoTowerConfirmationState();
        isShopOpen = !isShopOpen;
        UpdateShopUIState();

        if (!isShopOpen)
        {
            HideConsumableTooltip();
        }
    }

    public void OpenShop()
    {
        ClearNoTowerConfirmationState();
        isShopOpen = true;
        UpdateShopUIState();
        PopulateConsumableSlots();
        PopulateTowerSlots();
    }

    private void UpdateShopUIState()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(isShopOpen);
        }

        if (toggleButtonLabel != null)
        {
            toggleButtonLabel.text = isShopOpen ? closeShopText : openShopText;
        }

        if (nextRoundButton != null)
        {
            bool showNextRound = !isShopOpen;
            nextRoundButton.gameObject.SetActive(showNextRound);
            nextRoundButton.interactable = showNextRound;
        }
    }

    private void OnNextRoundButtonClicked()
    {
        if (awaitingNoTowerConfirmation && Time.unscaledTime > noTowerConfirmExpiresAt)
        {
            ClearNoTowerConfirmationState();
        }

        if (!HasAnyPlacedTower())
        {
            if (!awaitingNoTowerConfirmation)
            {
                BeginNoTowerConfirmationState();
                return;
            }
        }

        ClearNoTowerConfirmationState();
        isShopOpen = false;
        UpdateShopUIState();
        HideConsumableTooltip();
        ResetRerollCost();

        if (GameLoopManagerOld.Instance != null)
        {
            GameLoopManagerOld.Instance.OnNextRoundPressed();
        }
    }

    private void BeginNoTowerConfirmationState()
    {
        awaitingNoTowerConfirmation = true;
        noTowerConfirmExpiresAt = Time.unscaledTime + Mathf.Max(0.1f, noTowerConfirmWindowSeconds);
        Debug.LogWarning(noTowerConfirmWarning);
    }

    private void ClearNoTowerConfirmationState()
    {
        awaitingNoTowerConfirmation = false;
        noTowerConfirmExpiresAt = 0f;
    }

    private bool HasAnyPlacedTower()
    {
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
        }

        if (boardManager == null)
        {
            return true;
        }

        return boardManager.GetComponentsInChildren<BaseUnit>().Length > 0;
    }

    private void OnRerollButtonClicked()
    {
        if (currencyManager == null)
        {
            currencyManager = CurrencyManager.Instance;
        }

        if (currencyManager == null || !currencyManager.TrySpendCurrency(rerollCost))
        {
            return;
        }

        rerollCost++;
        UpdateRerollCostDisplay();
        PopulateConsumableSlots();
        PopulateTowerSlots();
    }

    public void ResetRerollCost()
    {
        rerollCost = 1;
        UpdateRerollCostDisplay();
    }

    private void UpdateRerollCostDisplay()
    {
        if (rerollCostText != null)
        {
            rerollCostText.text = $"Reroll: {rerollCost}";
        }
    }

    private void PopulateConsumableSlots()
    {
        if (availableItems == null || availableItems.Length == 0)
        {
            Debug.LogWarning("No available items assigned to populate consumable slots.");
            foreach (ConsumableSlot slot in consumableSlots)
            {
                if (slot != null)
                {
                    slot.Setup(null);
                }
            }
            return;
        }

        for (int i = 0; i < consumableSlots.Length; i++)
        {
            if (consumableSlots[i] == null) continue;
            ItemDefinition randomItem = availableItems[Random.Range(0, availableItems.Length)];
            consumableSlots[i].Setup(randomItem);
        }
    }

    private void PopulateTowerSlots()
    {
        if (databaseLoader == null || databaseLoader.UnitLookup == null || databaseLoader.UnitLookup.Count == 0)
        {
            Debug.LogWarning("[ShopManagerOld] Unit database is empty. Clearing tower slots.");
            foreach (TowerSlot slot in towerSlots)
            {
                if (slot != null) slot.Setup(null);
            }
            return;
        }

        List<string> unlockedUnitIDs = null;
        if (useProgressionFiltering && unlockManager != null)
        {
            unlockedUnitIDs = unlockManager.GetUnlockedUnitIDs();
        }

        List<UnitDefinition> availableUnits = new List<UnitDefinition>();
        foreach (UnitDefinition unitDef in databaseLoader.UnitLookup.Values)
        {
            if (unitDef == null)
            {
                continue;
            }

            if (useProgressionFiltering && unlockedUnitIDs != null && !unlockedUnitIDs.Contains(unitDef.UnitID))
            {
                continue;
            }

            availableUnits.Add(unitDef);
        }

        // We keep a persistent pool for the whole run, then build temporary draw buckets from what's left.
        Dictionary<UnitRarity, List<UnitDefinition>> rarityPools = CreateEmptyRarityPools();

        for (int i = 0; i < availableUnits.Count; i++)
        {
            AddUnitToRarityPool(availableUnits[i], rarityPools);
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
                Debug.Log($"[ShopManagerOld] Wave {GetCurrentShopRound()} slot {i + 1}: rolled {rolledRarity}, selected {randomUnit.UnitID} ({randomUnit.Name}).");
            }
        }
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
        return waveManager != null ? Mathf.Max(1, waveManager.currentWave) : 1;
    }

    public void ShowConsumableTooltip(string description)
    {
        if (!showTooltipOverlay)
        {
            return;
        }

        if (consumableTooltip == null || tooltipText == null)
        {
            return;
        }

        tooltipText.text = description;
        consumableTooltip.SetActive(true);
    }

    public void HideConsumableTooltip()
    {
        if (consumableTooltip != null)
        {
            consumableTooltip.SetActive(false);
        }
    }

    public bool UsesTooltipOverlay()
    {
        return showTooltipOverlay;
    }

    public void OnUnitPurchased(UnitDefinition unitDef)
    {
        if (unitDef == null)
        {
            return;
        }

        DecrementTowerPool(unitDef);
        Debug.Log($"[ShopManagerOld] Removed {unitDef.Name} from the remaining tower pool. Copies left: {GetRemainingTowerPoolCount(unitDef)}");
    }
}
