using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ConsumableSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button button;
    
    private ItemDefinition itemDefinition;
    private ShopManager shopManager;
    private ItemInventoryUI itemInventoryUI;
    
    private void Awake()
    {
        button.onClick.AddListener(OnSlotClicked);
    }
    
    public void Initialize(ShopManager manager)
    {
        shopManager = manager;
        Debug.Log($"ShopManager initialized for {gameObject.name}");
    }
    
    public void InitializeInventory(ItemInventoryUI inventory)
    {
        itemInventoryUI = inventory;
    }
    
    public void Setup(ItemDefinition data)
    {
        itemDefinition = data;
        
        if (data != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.color = Color.white;
            nameText.text = data.DisplayName;
            costText.text = $"Cost: {data.Cost}";
            button.interactable = true;
            
            Debug.Log($"Consumable slot setup: {data.DisplayName}");
        }
        else
        {
            ClearSlot();
        }
    }
    
    private void ClearSlot()
    {
        iconImage.sprite = null;
        iconImage.color = new Color(1, 1, 1, 0);
        nameText.text = "Empty";
        costText.text = "";
        button.interactable = false;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Pointer entered {gameObject.name}");
        
        if (itemDefinition != null && shopManager != null)
        {
            Debug.Log($"Showing tooltip for: {itemDefinition.DisplayName}");
            shopManager.ShowConsumableTooltip(itemDefinition.Description);
        }
        else
        {
            Debug.LogWarning($"Cannot show tooltip - itemDefinition: {itemDefinition != null}, shopManager: {shopManager != null}");
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Pointer exited {gameObject.name}");
        
        if (shopManager != null)
        {
            shopManager.HideConsumableTooltip();
        }
    }
    
    private void OnSlotClicked()
    {
        if (itemDefinition == null)
        {
            return;
        }

        if (itemInventoryUI == null)
        {
            Debug.LogWarning("ItemInventoryUI not assigned for consumable slot.");
            return;
        }

        // Check currency before purchase
        CurrencyManager currencyManager = CurrencyManager.Instance;
        if (currencyManager == null)
        {
            Debug.LogError("CurrencyManager not found!");
            return;
        }

        int cost = itemDefinition.Cost;

        // Try to spend currency
        if (!currencyManager.TrySpendCurrency(cost))
        {
            Debug.Log($"Cannot afford item! Need {cost}, have {currencyManager.GetCurrency()}");
            return;
        }

        // Add item to inventory
        if (itemInventoryUI.AddItem(itemDefinition))
        {
            Debug.Log($"Purchased {itemDefinition.DisplayName} for {cost} gold!");
        }
        else
        {
            Debug.Log("Item inventory is full.");
            // Refund currency if inventory is full
            currencyManager.AddCurrency(cost);
        }
    }
}