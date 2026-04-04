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

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayUiClick();
        }

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
        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayUiClick();
        }

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
        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayUiClick();
        }

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

        for (int i = 0; i < towerSlots.Length; i++)
        {
            if (towerSlots[i] == null)
            {
                continue;
            }

            if (availableUnits.Count == 0)
            {
                towerSlots[i].Setup(null);
                continue;
            }

            UnitDefinition randomUnit = availableUnits[Random.Range(0, availableUnits.Count)];
            towerSlots[i].Setup(randomUnit);
        }
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
}
