using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class TowerTooltipUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private Image towerIconImage;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI abilityPowerText;
    [SerializeField] private TextMeshProUGUI currentManaText;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI factionText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Tooltip Settings")]
    [SerializeField] private float offsetFromCursor = 20f;
    [SerializeField] private float screenEdgeBuffer = 10f;

    private UnitInstance currentDisplayedUnit;
    private Canvas canvas;
    
    // Static instance for easy access from other scripts
    public static TowerTooltipUI Instance { get; private set; }

    private void Awake()
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

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[TowerTooltipUI] No Canvas found in parent hierarchy!");
        }
    }

    private void Start()
    {
        // Ensure the tooltip starts hidden
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("[TowerTooltipUI] tooltipPanel NOT assigned in inspector!");
        }

        // Verify all required UI elements are assigned
        ValidateUIReferences();
        DisableTooltipRaycasts();
    }

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.gameObject.activeSelf)
        {
            UpdateTooltipPosition();
            if (currentDisplayedUnit != null)
            {
                UpdateTooltipDisplay();
            }
        }
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
        if (currentManaText == null)
            Debug.LogError("[TowerTooltipUI] currentManaText NOT assigned!");
        if (manaCostText == null)
            Debug.LogError("[TowerTooltipUI] manaCostText NOT assigned!");
        if (towerNameText == null)
            Debug.LogError("[TowerTooltipUI] towerNameText NOT assigned!");
        if (factionText == null)
            Debug.LogError("[TowerTooltipUI] factionText NOT assigned!");
        if (levelText == null)
            Debug.LogWarning("[TowerTooltipUI] levelText NOT assigned! Level will append to faction text.");
    }

    private void DisableTooltipRaycasts()
    {
        if (tooltipPanel == null)
        {
            return;
        }

        CanvasGroup group = tooltipPanel.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = tooltipPanel.gameObject.AddComponent<CanvasGroup>();
        }

        group.interactable = false;
        group.blocksRaycasts = false;
    }

    /// <summary>
    /// Displays the tooltip with the given tower's information
    /// </summary>
    public void ShowTooltip(UnitInstance unitInstance)
    {
        if (unitInstance == null)
        {
            Debug.LogWarning("[TowerTooltipUI] Attempted to show tooltip with null UnitInstance!");
            return;
        }

        currentDisplayedUnit = unitInstance;
        UpdateTooltipDisplay();

        // Ensure panel is visible
        if (tooltipPanel != null && !tooltipPanel.gameObject.activeSelf)
        {
            tooltipPanel.gameObject.SetActive(true);
        }

        UpdateTooltipPosition();
    }

    /// <summary>
    /// Hides the tooltip panel
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.gameObject.SetActive(false);
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

        int level = Mathf.Max(1, currentDisplayedUnit.Level);

        // Update faction and level
        if (factionText != null)
        {
            factionText.text = currentDisplayedUnit.Faction;
        }

        if (levelText != null)
        {
            levelText.text = $"LVL: {level}";
        }
        else if (factionText != null)
        {
            factionText.text = $"{currentDisplayedUnit.Faction}  LVL: {level}";
        }

        // Update tower icon
        if (towerIconImage != null && currentDisplayedUnit.BaseDef != null)
        {
            towerIconImage.sprite = currentDisplayedUnit.BaseDef.Icon;
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

        // Update stats
        if (healthText != null)
        {
            healthText.text = $"{currentDisplayedUnit.CurrentHP:F0}";
        }

        if (damageText != null)
        {
            damageText.text = $"{currentDisplayedUnit.GetModifiedDamage():F1}";
        }

        if (attackSpeedText != null)
        {
            attackSpeedText.text = $"{currentDisplayedUnit.GetModifiedAttackSpeed():F2}";
        }

        if (abilityPowerText != null)
        {
            abilityPowerText.text = $"{currentDisplayedUnit.GetModifiedAbilityPower():F1}";
        }

        if (currentManaText != null)
        {
            currentManaText.text = $"{currentDisplayedUnit.CurrentMana:F0}";
        }

        if (manaCostText != null && currentDisplayedUnit.BaseDef != null)
        {
            manaCostText.text = $"{currentDisplayedUnit.BaseDef.AbilityManaCost:F0}";
        }
    }

    /// <summary>
    /// Updates the tooltip's position to follow the cursor, adjusting for screen edges
    /// </summary>
    private void UpdateTooltipPosition()
    {
        if (tooltipPanel == null || canvas == null) return;

        // Use new Input System to get mouse position
        if (Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPoint
        );

        Vector2 tooltipSize = tooltipPanel.sizeDelta;
        Vector2 canvasSize = (canvas.transform as RectTransform).sizeDelta;

        float halfScreenWidth = canvasSize.x / 2f;
        bool showOnRight = localPoint.x < 0;

        Vector2 tooltipPosition = localPoint;

        if (showOnRight)
        {
            tooltipPosition.x += offsetFromCursor;
            tooltipPosition.x = Mathf.Min(tooltipPosition.x, halfScreenWidth - tooltipSize.x / 2f - screenEdgeBuffer);
        }
        else
        {
            tooltipPosition.x -= offsetFromCursor;
            tooltipPosition.x = Mathf.Max(tooltipPosition.x, -halfScreenWidth + tooltipSize.x / 2f + screenEdgeBuffer);
        }

        float halfScreenHeight = canvasSize.y / 2f;
        tooltipPosition.y = Mathf.Clamp(
            tooltipPosition.y,
            -halfScreenHeight + tooltipSize.y / 2f + screenEdgeBuffer,
            halfScreenHeight - tooltipSize.y / 2f - screenEdgeBuffer
        );

        tooltipPanel.localPosition = tooltipPosition;
    }

    /// <summary>
    /// Gets the currently displayed unit (useful for checking if a new tooltip should be shown)
    /// </summary>
    public UnitInstance GetCurrentDisplayedUnit()
    {
        return currentDisplayedUnit;
    }

    /// <summary>
    /// Checks if the tooltip is currently visible
    /// </summary>
    public bool IsTooltipVisible()
    {
        return tooltipPanel != null && tooltipPanel.gameObject.activeSelf;
    }
}
