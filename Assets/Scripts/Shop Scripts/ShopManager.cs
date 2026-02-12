using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button nextRoundButton;
    
    [Header("Shop Slots")]
    [SerializeField] private ConsumableSlot[] consumableSlots = new ConsumableSlot[2];
    [SerializeField] private TowerSlot[] towerSlots = new TowerSlot[6];

    [Header("Data Sources")]
    [SerializeField] private DatabaseLoader databaseLoader;
    [SerializeField] private InventoryUI inventoryUI;
    
    [Header("Tooltip")]
    [SerializeField] private GameObject consumableTooltip;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    [Header("Test Data")]
    [SerializeField] private ConsumableData[] testConsumableData;
    
    private bool isShopOpen = false;
    
    private void Start()
    {
        toggleButton.onClick.AddListener(ToggleShop);
        nextRoundButton.onClick.AddListener(OnNextRoundButtonClicked);
        rerollButton.onClick.AddListener(RerollShop);
        
        shopPanel.SetActive(false);
        consumableTooltip.SetActive(false);
        
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

        foreach (TowerSlot slot in towerSlots)
        {
            if (slot != null)
            {
                slot.Initialize(inventoryUI);
            }
        }
        
        // Setup consumable slots with test data
        if (testConsumableData != null && testConsumableData.Length > 0)
        {
            Debug.Log($"Setting up {testConsumableData.Length} consumable slots");
            for (int i = 0; i < consumableSlots.Length && i < testConsumableData.Length; i++)
            {
                if (consumableSlots[i] != null && testConsumableData[i] != null)
                {
                    consumableSlots[i].Setup(testConsumableData[i]);
                }
                else
                {
                    Debug.LogWarning($"Consumable slot {i} or data is null - Slot: {consumableSlots[i] != null}, Data: {testConsumableData[i] != null}");
                }
            }
        }
        else
        {
            Debug.LogWarning("testConsumableData is null or empty! No consumable slots will be populated.");
        }

        if (databaseLoader == null)
        {
            databaseLoader = FindFirstObjectByType<DatabaseLoader>();
        }

        PopulateTowerSlots();
    }
    
    public void ToggleShop()
    {
        isShopOpen = !isShopOpen;
        shopPanel.SetActive(isShopOpen);
        
        if (!isShopOpen)
        {
            HideConsumableTooltip();
        }
    }

    // method for gameloopmanger to open a new shop at the start of each round - also rerolls the shop to show new options
    public void OpenShop()
    {
        isShopOpen = true;
        shopPanel.SetActive(true);
        toggleButton.interactable = true;
        
        // Reroll shop for new round
        PopulateTowerSlots();
    }
    
    private void OnNextRoundButtonClicked()
    {
        // Close shop
        shopPanel.SetActive(false);
        isShopOpen = false;
        HideConsumableTooltip();
        
        // Notify game loop manager
        if (GameLoopManager.Instance != null)
        {
            GameLoopManager.Instance.OnNextRoundPressed();
        }
    }
    
    private void RerollShop()
    {
        PopulateTowerSlots();
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
        foreach (var unitDef in databaseLoader.UnitLookup.Values)
        {
            if (HasValidPrefab(unitDef))
            {
                eligibleUnits.Add(unitDef);
            }
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
}