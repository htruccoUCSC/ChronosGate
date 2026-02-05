using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button nextRoundButton;
    
    [Header("Shop Slots")]
    [SerializeField] private ConsumableSlot[] consumableSlots;
    [SerializeField] private TowerSlot[] towerSlots;
    
    private bool isShopOpen = false;
    private bool shopUsed = false; // Track if shop was closed via Next Round
    
    private void Start()
    {
        // Setup button listeners
        toggleButton.onClick.AddListener(ToggleShop);
        nextRoundButton.onClick.AddListener(CloseShopPermanently);
        rerollButton.onClick.AddListener(RerollShop);
        
        // Initially hide shop
        shopPanel.SetActive(false);
    }
    
    public void ToggleShop()
    {
        if (shopUsed) return; // Don't allow reopening after Next Round
        
        isShopOpen = !isShopOpen;
        shopPanel.SetActive(isShopOpen);
    }
    
    private void CloseShopPermanently()
    {
        shopUsed = true;
        shopPanel.SetActive(false);
        
        // Optional: Disable or hide the toggle button
        toggleButton.interactable = false;
        // OR: toggleButton.gameObject.SetActive(false);
    }
    
    private void RerollShop()
    {
        // Placeholder for future implementation
        Debug.Log("Reroll functionality to be implemented");
        
        // When implemented, this will:
        // 1. Generate new random consumables
        // 2. Generate new random towers
        // 3. Update all shop slots
    }
}