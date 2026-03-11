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
    private ShopManagerOld shopManagerOld;
    private ItemInventoryUI itemInventoryUI;
    
    private void Awake()
    {
        AutoAssignReferences();

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

    private void AutoAssignReferences()
    {
        if (nameText == null || costText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI text in texts)
            {
                string lowerName = text.gameObject.name.ToLower();
                if (nameText == null && lowerName.Contains("name"))
                {
                    nameText = text;
                    continue;
                }

                if (costText == null && lowerName.Contains("cost"))
                {
                    costText = text;
                }
            }
        }

        if (iconImage == null)
        {
            Image[] images = GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                if (image != null && image.gameObject.name.ToLower().Contains("icon"))
                {
                    iconImage = image;
                    break;
                }
            }
        }
    }
    
    public void Initialize(ShopManager manager)
    {
        shopManager = manager;
        shopManagerOld = null;
        Debug.Log($"ShopManager initialized for {gameObject.name}");
    }

    public void Initialize(ShopManagerOld manager)
    {
        shopManagerOld = manager;
        shopManager = null;
        Debug.Log($"ShopManager initialized for {gameObject.name}");
    }
    
    public void InitializeInventory(ItemInventoryUI inventory)
    {
        itemInventoryUI = inventory;
    }
    
    public void Setup(ItemDefinition data)
    {
        itemDefinition = data;
        AutoAssignReferences();
        
        if (data != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.color = iconImage.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
            if (nameText != null) nameText.text = data.DisplayName;
            if (costText != null) costText.text = $"{data.Cost}";
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
        AutoAssignReferences();
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
        if (itemDefinition == null)
        {
            return;
        }

        if (shopManager != null)
        {
            shopManager.ShowConsumableTooltip(itemDefinition.Description);
            return;
        }

        if (shopManagerOld != null)
        {
            if (shopManagerOld.UsesTooltipOverlay())
            {
                shopManagerOld.ShowConsumableTooltip(itemDefinition.Description);
            }
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Pointer exited {gameObject.name}");
        
        if (shopManager != null)
        {
            shopManager.HideConsumableTooltip();
            return;
        }

        if (shopManagerOld != null)
        {
            if (shopManagerOld.UsesTooltipOverlay())
            {
                shopManagerOld.HideConsumableTooltip();
            }
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
