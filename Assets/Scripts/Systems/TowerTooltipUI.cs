using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerTooltipUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private Image towerIconImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI abilityPowerText;
    [SerializeField] private TextMeshProUGUI manaPerShotText;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI factionText;

    private UnitDefinition currentDisplayedUnit;
    
    // Static instance for easy access from other scripts
    public static TowerTooltipUI Instance { get; private set; }

    private void Start()
    {
        // Register as singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogWarning("[TowerTooltipUI] Multiple instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Ensure the tooltip starts hidden
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[TowerTooltipUI] tooltipPanel NOT assigned in inspector!");
        }

        // Verify all required UI elements are assigned
        ValidateUIReferences();
    }

    private void ValidateUIReferences()
    {
        if (towerIconImage == null)
            Debug.LogError("[TowerTooltipUI] towerIconImage NOT assigned!");
        if (healthText == null)
            Debug.LogError("[TowerTooltipUI] healthText NOT assigned!");
        if (damageText == null)
            Debug.LogError("[TowerTooltipUI] damageText NOT assigned!");
        if (attackSpeedText == null)
            Debug.LogError("[TowerTooltipUI] attackSpeedText NOT assigned!");
        if (abilityPowerText == null)
            Debug.LogError("[TowerTooltipUI] abilityPowerText NOT assigned!");
        if (manaPerShotText == null)
            Debug.LogError("[TowerTooltipUI] manaPerShotText NOT assigned!");
        if (manaCostText == null)
            Debug.LogError("[TowerTooltipUI] manaCostText NOT assigned!");
        if (towerNameText == null)
            Debug.LogError("[TowerTooltipUI] towerNameText NOT assigned!");
        if (factionText == null)
            Debug.LogError("[TowerTooltipUI] factionText NOT assigned!");
    }

    /// <summary>
    /// Displays the tooltip with the given tower's information
    /// </summary>
    public void ShowTooltip(UnitDefinition unitDef)
    {
        if (unitDef == null)
        {
            Debug.LogWarning("[TowerTooltipUI] Attempted to show tooltip with null UnitDefinition!");
            return;
        }

        // Only update if it's a different tower or first time
        if (currentDisplayedUnit != unitDef)
        {
            currentDisplayedUnit = unitDef;
            UpdateTooltipDisplay();
        }

        // Ensure panel is visible
        if (tooltipPanel != null && !tooltipPanel.activeSelf)
        {
            tooltipPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Hides the tooltip panel
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        currentDisplayedUnit = null;
    }

    /// <summary>
    /// Updates all UI elements with the current unit definition data
    /// </summary>
    private void UpdateTooltipDisplay()
    {
        if (currentDisplayedUnit == null) return;

        // Update tower name
        if (towerNameText != null)
        {
            towerNameText.text = currentDisplayedUnit.Name;
        }

        // Update faction
        if (factionText != null)
        {
            factionText.text = currentDisplayedUnit.Faction;
        }

        // Update tower icon
        if (towerIconImage != null)
        {
            towerIconImage.sprite = currentDisplayedUnit.Icon;
            if (towerIconImage.sprite == null)
            {
                Debug.LogWarning($"[TowerTooltipUI] No icon found for tower: {currentDisplayedUnit.Name}");
                towerIconImage.color = new Color(1, 1, 1, 0.5f); // Make it slightly transparent if no icon
            }
            else
            {
                towerIconImage.color = Color.white;
            }
        }

        // Update stats with abbreviated labels
        if (healthText != null)
        {
            healthText.text = $"HP: {currentDisplayedUnit.Health}";
        }

        if (damageText != null)
        {
            damageText.text = $"DMG: {currentDisplayedUnit.AttackDamage:F1}";
        }

        if (attackSpeedText != null)
        {
            attackSpeedText.text = $"AS: {currentDisplayedUnit.AttackSpeed:F2}";
        }

        if (abilityPowerText != null)
        {
            abilityPowerText.text = $"AP: {currentDisplayedUnit.AbilityPower:F1}";
        }

        if (manaPerShotText != null)
        {
            manaPerShotText.text = $"Mana/Shot: {currentDisplayedUnit.ManaPerShot:F1}";
        }

        if (manaCostText != null)
        {
            manaCostText.text = $"Ability Cost: {currentDisplayedUnit.AbilityManaCost:F1}";
        }
    }

    /// <summary>
    /// Gets the currently displayed unit (useful for checking if a new tooltip should be shown)
    /// </summary>
    public UnitDefinition GetCurrentDisplayedUnit()
    {
        return currentDisplayedUnit;
    }

    /// <summary>
    /// Checks if the tooltip is currently visible
    /// </summary>
    public bool IsTooltipVisible()
    {
        return tooltipPanel != null && tooltipPanel.activeSelf;
    }
}
