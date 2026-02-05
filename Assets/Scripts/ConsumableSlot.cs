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
    
    private ConsumableData consumableData;
    private ShopManager shopManager;
    
    private void Awake()
    {
        button.onClick.AddListener(OnSlotClicked);
        shopManager = GetComponentInParent<ShopManager>();
    }
    
    public void Setup(ConsumableData data)
    {
        consumableData = data;
        
        if (data != null)
        {
            iconImage.sprite = data.icon;
            iconImage.color = Color.white;
            nameText.text = data.consumableName;
            costText.text = $"Cost: {data.cost}";
            button.interactable = true;
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
        if (consumableData != null && shopManager != null)
        {
            shopManager.ShowConsumableTooltip(consumableData.description);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (shopManager != null)
        {
            shopManager.HideConsumableTooltip();
        }
    }
    
    private void OnSlotClicked()
    {
        if (consumableData != null)
        {
            Debug.Log($"Clicked consumable: {consumableData.consumableName}");
            // Future: Check cost, deduct currency, add to inventory
        }
    }
}