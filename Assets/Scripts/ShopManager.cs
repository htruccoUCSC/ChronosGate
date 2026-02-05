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
    
    private bool isShopOpen = false;
    private bool shopUsed = false;
    
    private void Start()
    {
        toggleButton.onClick.AddListener(ToggleShop);
        nextRoundButton.onClick.AddListener(CloseShopPermanently);
        rerollButton.onClick.AddListener(RerollShop);
        
        shopPanel.SetActive(false);
        consumableTooltip.SetActive(false); // Hide tooltip initially
    }
    
    public void ToggleShop()
    {
        if (shopUsed) return;
        
        isShopOpen = !isShopOpen;
        shopPanel.SetActive(isShopOpen);
        
        if (!isShopOpen)
        {
            HideConsumableTooltip(); // Hide tooltip when closing shop
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
    
    // NEW: Show consumable tooltip
    public void ShowConsumableTooltip(string description)
    {
        if (consumableTooltip != null && tooltipText != null)
        {
            tooltipText.text = description;
            consumableTooltip.SetActive(true);
        }
    }
    
    // NEW: Hide consumable tooltip
    public void HideConsumableTooltip()
    {
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