using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    }

    private void OnSlotClicked()
    {
        if (unitDefinition == null)
        {
            return;
        }

        if (inventoryUI == null)
        {
            Debug.LogWarning("InventoryUI not assigned for tower slot.");
            return;
        }

        if (inventoryUI.AddUnit(unitDefinition))
        {
            Setup(null);
        }
        else
        {
            Debug.Log("Inventory is full.");
        }
    }
}