using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CurrencyUI : MonoBehaviour
{
    public static CurrencyUI Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI currencyText;
    private CurrencyManager currencyManager;
    private Canvas m_Canvas;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }

        Instance = this;
        m_Canvas = GetComponentInParent<Canvas>();
    }

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
            currencyText.text = $"{newCurrency}";
        }
    }

    // Expose the position of the currency text for coin vfx
    public RectTransform CurrencyTextRect => currencyText != null ? currencyText.rectTransform : null;

    public void PlayCurrencyGainAnimation(Vector3 worldOrigin, int amount)
    {
        if (amount <= 0 || currencyText == null || m_Canvas == null)
        {
            return;
        }

        CurrencyFlyVfx.Spawn(m_Canvas, currencyText.rectTransform, worldOrigin, amount);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
        }
    }
}
