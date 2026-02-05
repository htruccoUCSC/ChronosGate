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
    
    [Header("Tooltip (Optional)")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    private ConsumableData consumableData;
    
    private void Awake()
    {
        button.onClick.AddListener(OnSlotClicked);
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
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
        if (consumableData != null && tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            tooltipText.text = consumableData.description;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
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