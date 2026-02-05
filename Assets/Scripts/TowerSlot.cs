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
    
    private TowerData towerData;
    
    private void Awake()
    {
        button.onClick.AddListener(OnSlotClicked);
    }
    
    public void Setup(TowerData data)
    {
        towerData = data;
        
        if (data != null)
        {
            eraText.text = data.era;
            towerNameText.text = data.towerName;
            typeText.text = data.type;
            iconImage.sprite = data.icon;
            iconImage.color = Color.white;
            costText.text = $"Cost: {data.cost}";
            descriptionText.text = data.description;
            button.interactable = true;
        }
        else
        {
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
        if (towerData != null)
        {
            Debug.Log($"Clicked tower: {towerData.towerName}");
            // Future: Check cost, deduct currency, add to inventory
        }
    }
}