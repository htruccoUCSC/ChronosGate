using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    
    [Header("Tooltip")]
    [SerializeField] private GameObject consumableTooltip;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    [Header("Test Data")]
    [SerializeField] private ConsumableData[] testConsumableData;
    [SerializeField] private TowerData[] testTowerData; // ADD THIS
    
    private bool isShopOpen = false;
    private bool shopUsed = false;
    
    private void Start()
    {
        toggleButton.onClick.AddListener(ToggleShop);
        nextRoundButton.onClick.AddListener(CloseShopPermanently);
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
        
        // NEW: Setup tower slots with test data
        if (testTowerData != null && testTowerData.Length > 0)
        {
            for (int i = 0; i < towerSlots.Length && i < testTowerData.Length; i++)
            {
                if (towerSlots[i] != null && testTowerData[i] != null)
                {
                    Debug.Log($"Setting up tower slot {i} with {testTowerData[i].towerName}");
                    towerSlots[i].Setup(testTowerData[i]);
                }
                else
                {
                    Debug.LogWarning($"Tower slot {i} or data is null - Slot: {towerSlots[i] != null}, Data: {testTowerData[i] != null}");
                }
            }
        }
        else
        {
            Debug.LogWarning("No test tower data assigned!");
        }
    }
    
    public void ToggleShop()
    {
        if (shopUsed) return;
        
        isShopOpen = !isShopOpen;
        shopPanel.SetActive(isShopOpen);
        
        if (!isShopOpen)
        {
            HideConsumableTooltip();
        }
    }
    
    private void CloseShopPermanently()
    {
        shopUsed = true;
        shopPanel.SetActive(false);
        HideConsumableTooltip();
        
        toggleButton.interactable = false;
    }
    
    private void RerollShop()
    {
        Debug.Log("Reroll functionality to be implemented");
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