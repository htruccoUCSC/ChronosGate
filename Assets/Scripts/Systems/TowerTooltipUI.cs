using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI modifiersText;

    [Header("Tooltip Settings")]
    [SerializeField] private float animateToCenterDuration = 0.35f;

    private UnitInstance currentDisplayedUnit;
    private Canvas canvas;
    private bool m_IsOpen = false;
    private Coroutine m_AnimateCoroutine;
    
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
        if (tooltipPanel != null)
        {
            // If tooltipPanel is a child object, deactivate it safely.
            // If it's this same GameObject, we cannot deactivate it — hide via CanvasGroup instead.
            if (tooltipPanel.gameObject != this.gameObject)
                tooltipPanel.gameObject.SetActive(false);
            else
                DisableTooltipRaycasts(); // CanvasGroup alpha=0 handled below
        }
        else
        {
            Debug.LogError("[TowerTooltipUI] tooltipPanel NOT assigned in inspector!");
        }

        ValidateUIReferences();
        DisableTooltipRaycasts();

        // Hide panel visually if it shares this GO
        if (tooltipPanel != null && tooltipPanel.gameObject == this.gameObject)
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        // Live-update stats while open
        if (m_IsOpen && currentDisplayedUnit != null)
            UpdateTooltipDisplay();
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
        if (rarityText == null)
            Debug.LogWarning("[TowerTooltipUI] rarityText NOT assigned!");
        if (rangeText == null)
            Debug.LogWarning("[TowerTooltipUI] rangeText NOT assigned!");
        if (descriptionText == null)
            Debug.LogWarning("[TowerTooltipUI] descriptionText NOT assigned!");
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
    /// Opens the tooltip: snap to cursor, pause, dim, hide other UI, then animate to center.
    /// </summary>
    public void ShowTooltip(UnitInstance unitInstance)
    {
        if (unitInstance == null) return;

        currentDisplayedUnit = unitInstance;
        m_IsOpen = true;
        UpdateTooltipDisplay();

        if (tooltipPanel != null)
        {
            if (tooltipPanel.gameObject != this.gameObject)
                tooltipPanel.gameObject.SetActive(true);
            else
            {
                var cg = GetComponent<CanvasGroup>();
                if (cg != null) { cg.alpha = 1f; cg.blocksRaycasts = false; cg.interactable = false; }
            }
        }

        // Ensure this GameObject is active so coroutines can run
        // (tooltipPanel may be the same GO as TowerTooltipUI's own GameObject)
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Place at cursor first
        SnapToCursor();

        // Pause the game
        if (GameSpeedButton.Instance != null)
            GameSpeedButton.Instance.SetPaused(true);

        // Hide all other canvas siblings
        SetOtherUIVisible(false);

        // Animate to center
        if (m_AnimateCoroutine != null) StopCoroutine(m_AnimateCoroutine);
        m_AnimateCoroutine = StartCoroutine(AnimateToCenter());
    }

    /// <summary>
    /// Closes the tooltip and restores game state.
    /// </summary>
    public void HideTooltip()
    {
        if (!m_IsOpen) return;
        m_IsOpen = false;
        currentDisplayedUnit = null;

        if (m_AnimateCoroutine != null) { StopCoroutine(m_AnimateCoroutine); m_AnimateCoroutine = null; }

        if (tooltipPanel != null)
        {
            if (tooltipPanel.gameObject != this.gameObject)
                tooltipPanel.gameObject.SetActive(false);
            else
            {
                var cg = GetComponent<CanvasGroup>();
                if (cg != null) { cg.alpha = 0f; cg.blocksRaycasts = false; cg.interactable = false; }
            }
        }

        // Resume game
        if (GameSpeedButton.Instance != null)
            GameSpeedButton.Instance.SetPaused(false);

        // Restore other UI
        SetOtherUIVisible(true);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void SnapToCursor()
    {
        if (tooltipPanel == null || canvas == null) return;
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, mousePos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out Vector2 local);
        tooltipPanel.localPosition = local;
    }

    private IEnumerator AnimateToCenter()
    {
        if (tooltipPanel == null) yield break;

        Vector3 startPos = tooltipPanel.localPosition;
        Vector3 endPos = Vector3.zero; // canvas center
        float elapsed = 0f;

        while (elapsed < animateToCenterDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / animateToCenterDuration);
            tooltipPanel.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        tooltipPanel.localPosition = endPos;
        m_AnimateCoroutine = null;
    }

    private readonly List<GameObject> m_HiddenUI = new List<GameObject>();

    private void SetOtherUIVisible(bool visible)
    {
        // WaveCycleProgressUI re-shows itself every Update frame, so handle it explicitly.
        var waveProgress = FindFirstObjectByType<WaveCycleProgressUI>();
        if (waveProgress != null)
            waveProgress.SetForceHidden(!visible);

        if (canvas == null) return;

        if (!visible)
        {
            // Find the direct canvas child that contains this component (may be a parent of us).
            Transform selfRoot = transform;
            while (selfRoot.parent != null && selfRoot.parent != canvas.transform)
                selfRoot = selfRoot.parent;

            m_HiddenUI.Clear();
            foreach (Transform child in canvas.transform)
            {
                if (child == tooltipPanel.transform) continue;
                if (child == selfRoot) continue;
                if (!child.gameObject.activeSelf) continue;
                m_HiddenUI.Add(child.gameObject);
                child.gameObject.SetActive(false);
            }
        }
        else
        {
            foreach (var go in m_HiddenUI)
            {
                if (go != null) go.SetActive(true);
            }
            m_HiddenUI.Clear();
        }
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
            levelText.text = $"{level}";
        }
        else if (factionText != null)
        {
            factionText.text = $"{currentDisplayedUnit.Faction}  {level}";
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

        if (rangeText != null)
        {
            rangeText.text = $"{currentDisplayedUnit.GetModifiedRange():F1}";
        }

        if (rarityText != null && currentDisplayedUnit.BaseDef != null)
        {
            rarityText.text = currentDisplayedUnit.BaseDef.Rarity.ToString();
        }

        if (descriptionText != null && currentDisplayedUnit.BaseDef != null)
        {
            descriptionText.text = currentDisplayedUnit.BaseDef.Description;
        }

        if (modifiersText != null)
        {
            modifiersText.text = BuildModifiersString(currentDisplayedUnit);
        }
    }

    private string BuildModifiersString(UnitInstance unit)
    {
        var sb = new System.Text.StringBuilder();
        if (unit.DamageMultMod != 1f || unit.DamageFlatMod != 0f)
            sb.AppendLine($"DMG ×{unit.DamageMultMod:F2}  +{unit.DamageFlatMod:F0}");
        if (unit.SpeedMultMod != 1f || unit.SpeedFlatMod != 0f)
            sb.AppendLine($"SPD ×{unit.SpeedMultMod:F2}  +{unit.SpeedFlatMod:F2}");
        if (unit.AbilityPowerMult != 1f || unit.AbilityPowerFlatMod != 0f)
            sb.AppendLine($"AP ×{unit.AbilityPowerMult:F2}  +{unit.AbilityPowerFlatMod:F1}");
        if (unit.RangeFlatMod != 0f)
            sb.AppendLine($"RNG +{unit.RangeFlatMod:F1}");
        return sb.Length > 0 ? sb.ToString().TrimEnd() : "None";
    }

    // UpdateTooltipPosition is no longer used (tooltip animates to center on open).

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
