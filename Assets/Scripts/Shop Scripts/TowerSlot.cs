using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TowerSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI eraText;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button button;
    [SerializeField] private bool active = false;

    private UnitDefinition unitDefinition;
    private InventoryUI inventoryUI;
    private ShopManager shopManager;
    private ShopManagerOld shopManagerOld;

    private void Awake()
    {
        // Only proceed if this is actually a TowerSlot with required components
        if (button == null || eraText == null)
        {
            Debug.LogWarning($"[TowerSlot] {gameObject.name} is missing required components. Skipping tooltip setup.");
            return;
        }
        
        button.onClick.RemoveListener(OnSlotClicked);
        button.onClick.AddListener(OnSlotClicked);
    }

    public void Initialize(InventoryUI inventory)
    {
        inventoryUI = inventory;
        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }
        if (shopManagerOld == null)
        {
            shopManagerOld = FindFirstObjectByType<ShopManagerOld>();
        }
    }

    public void Setup(UnitDefinition data)
    {
        unitDefinition = data;

        if (data != null)
        {
            bool useInlineDescription = shopManagerOld != null && shopManager == null;

            if (eraText != null) eraText.text = data.Faction;
            if (towerNameText != null) towerNameText.text = data.Name;
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.color = iconImage.sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
            if (costText != null) costText.text = $"{data.Cost}";
            if (descriptionText != null) descriptionText.text = useInlineDescription ? data.Description : "";
            if (button != null) button.interactable = true;
            active = true;

            // Set faction color on background image
            if (backgroundImage != null)
            {
                backgroundImage.color = GetFactionColor(data.Faction);
            }

            Debug.Log($"Tower slot setup complete for: {data.Name}");
        }
        else
        {
            Debug.LogWarning($"UnitDefinition is null for {gameObject.name}");

            ClearSlot();
        }
    }

    private void ClearSlot()
    {
        if (eraText != null) eraText.text = "";
        if (towerNameText != null) towerNameText.text = "Empty";
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = new Color(1, 1, 1, 0);
        }
        if (costText != null) costText.text = "";
        if (descriptionText != null) descriptionText.text = "";
        if (button != null) button.interactable = false;
        active = false;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
        }
    }

    private void OnSlotClicked()
    {
        if (active == false)
        {
            return;
        }
        if (unitDefinition == null)
        {
            return;
        }

        if (inventoryUI == null)
        {
            Debug.LogWarning("InventoryUI not assigned for tower slot.");
            return;
        }

        // Check currency before purchase
        CurrencyManager currencyManager = CurrencyManager.Instance;
        if (currencyManager == null)
        {
            Debug.LogError("CurrencyManager not found!");
            return;
        }

        // Try to spend currency
        if (!currencyManager.TrySpendCurrency(unitDefinition.Cost))
        {
            Debug.Log($"Cannot afford unit! Need {unitDefinition.Cost}, have {currencyManager.GetCurrency()}");
            return;
        }

        // Add unit to inventory
        if (inventoryUI.AddUnit(unitDefinition))
        {
            Debug.Log($"Purchased {unitDefinition.Name} for {unitDefinition.Cost} gold!");
            ClearSlot();
        }
        else
        {
            Debug.Log("Inventory is full.");
            // Refund currency if inventory is full
            currencyManager.AddCurrency(unitDefinition.Cost);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unitDefinition == null)
        {
            return;
        }

        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }
        if (shopManagerOld == null)
        {
            shopManagerOld = FindFirstObjectByType<ShopManagerOld>();
        }

        if (shopManager != null)
        {
            shopManager.ShowConsumableTooltip(unitDefinition.Description);
            return;
        }

        if (shopManagerOld != null)
        {
            if (shopManagerOld.UsesTooltipOverlay())
            {
                shopManagerOld.ShowConsumableTooltip(unitDefinition.Description);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }
        if (shopManagerOld == null)
        {
            shopManagerOld = FindFirstObjectByType<ShopManagerOld>();
        }

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

    private Color GetFactionColor(string faction)
    {
        switch (faction)
        {
            case "Prehistoric":
                return new Color(0.6f, 0.4f, 0.2f); // Brown
            case "Fantasy":
                return new Color(0.5f, 0.3f, 0.7f); // Purple
            case "Medieval":
                return new Color(0.7f, 0.7f, 0.7f); // Gray
            case "Mystic":
                return new Color(0.2f, 0.6f, 0.8f); // Light Blue
            case "Modern":
                return new Color(0.3f, 0.3f, 0.3f); // Dark Gray
            case "Future":
                return new Color(0.0f, 0.8f, 0.4f); // Green
            case "Cosmic":
                return new Color(0.5f, 0.0f, 0.8f); // Deep Purple
            default:
                return Color.white;
        }
    }
}
