using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TowerSlot : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI eraText;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button button;
    [SerializeField] private bool active = false;

    private UnitDefinition unitDefinition;
    private InventoryUI inventoryUI;

    private void Awake()
    {
        button.onClick.AddListener(OnSlotClicked);
    }

    public void Initialize(InventoryUI inventory)
    {
        inventoryUI = inventory;
    }

    public void Setup(UnitDefinition data)
    {
        unitDefinition = data;

        if (data != null)
        {
            eraText.text = data.Faction;
            towerNameText.text = data.Name;
            typeText.text = data.AttackFunction.ToString();
            iconImage.sprite = data.Icon;
            iconImage.color = Color.white;
            costText.text = $"Cost: {data.Cost}";
            descriptionText.text = data.Description;
            button.interactable = true;
            active = true;

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
        eraText.text = "";
        towerNameText.text = "Empty";
        typeText.text = "";
        iconImage.sprite = null;
        iconImage.color = new Color(1, 1, 1, 0);
        costText.text = "";
        descriptionText.text = "";
        button.interactable = false;
        active = false;
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
}