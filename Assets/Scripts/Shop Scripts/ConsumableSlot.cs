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
    }
    
    // NEW: Let ShopManager set itself
    public void Initialize(ShopManager manager)
    {
        shopManager = manager;
        Debug.Log($"ShopManager initialized for {gameObject.name}");
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
            
            Debug.Log($"Consumable slot setup: {data.consumableName}");
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
        
        if (consumableData != null && shopManager != null)
        {
            Debug.Log($"Showing tooltip for: {consumableData.consumableName}");
            shopManager.ShowConsumableTooltip(consumableData.description);
        }
        else
        {
            Debug.LogWarning($"Cannot show tooltip - consumableData: {consumableData != null}, shopManager: {shopManager != null}");
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
        if (consumableData != null)
        {
            Debug.Log($"Clicked consumable: {consumableData.consumableName}");
        }
    }
}