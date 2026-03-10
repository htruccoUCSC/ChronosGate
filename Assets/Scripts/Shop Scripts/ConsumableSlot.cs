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
        if (button == null)
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                button = GetComponentInChildren<Button>(true);
            }
        }

        if (button != null)
        {
            button.onClick.RemoveListener(OnSlotClicked);
            button.onClick.AddListener(OnSlotClicked);
        }
        else
        {
            Debug.LogWarning($"[ConsumableSlot] {gameObject.name} is missing a Button reference.");
        }
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
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.color = iconImage.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
            if (nameText != null) nameText.text = data.DisplayName;
            if (costText != null) costText.text = $"Cost: {data.Cost}";
            if (button != null) button.interactable = true;
            
            Debug.Log($"Consumable slot setup: {data.DisplayName}");
        }
        else
        {
            ClearSlot();
        }
    }
    
    private void ClearSlot()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
        }
        if (nameText != null) nameText.text = "Empty";
        if (costText != null) costText.text = "";
        if (button != null) button.interactable = false;
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
            itemDefinition = null;
            ClearSlot();
        }
        else
        {
            Debug.Log("Item inventory is full.");
            // Refund currency if inventory is full
            currencyManager.AddCurrency(cost);
        }
    }
}
