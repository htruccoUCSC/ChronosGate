using UnityEngine;
using TMPro;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;
    private CurrencyManager currencyManager;
    
    private void Start()
    {
        currencyManager = CurrencyManager.Instance;
        
        if (currencyManager == null)
        {
            Debug.LogError("CurrencyManager Instance not found!");
            return;
        }
        
        // Subscribe to currency changes
        currencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
        
        // Initial update
        UpdateCurrencyDisplay(currencyManager.GetCurrency());
    }
    
    private void UpdateCurrencyDisplay(int newCurrency)
    {
        if (currencyText != null)
        {
            currencyText.text = $"Gold: {newCurrency}";
        }
    }
    
    private void OnDestroy()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
        }
    }
}