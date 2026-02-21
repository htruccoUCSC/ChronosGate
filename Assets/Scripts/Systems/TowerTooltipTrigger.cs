using UnityEngine;
using UnityEngine.EventSystems;

public class TowerTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UnitDefinition unitDefinition;
    private TowerTooltipUI tooltipUI;
    private bool isHovering = false;

    private void Start()
    {
        // TowerTooltipUI will register itself as Instance when it initializes
        // No need to search for it here
    }

    /// <summary>
    /// Set the TowerTooltipUI reference directly (called by TowerSlot)
    /// </summary>
    public void SetTooltipUI(TowerTooltipUI ui)
    {
        tooltipUI = ui;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        // Silently return if no unit is assigned (this trigger might not be in use)
        if (unitDefinition == null)
        {
            return;
        }

        // Use the static instance
        if (TowerTooltipUI.Instance != null)
        {
            TowerTooltipUI.Instance.ShowTooltip(unitDefinition);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        // Tooltip now persists - do NOT hide it when moving away
        // Only another tower's OnPointerEnter will update it
    }

    /// <summary>
    /// Set the unit definition for this trigger (useful if you want to set it dynamically)
    /// </summary>
    public void SetUnitDefinition(UnitDefinition unitDef)
    {
        unitDefinition = unitDef;
    }

    /// <summary>
    /// Get the unit definition this trigger is associated with
    /// </summary>
    public UnitDefinition GetUnitDefinition()
    {
        return unitDefinition;
    }

    /// <summary>
    /// Check if mouse is currently hovering over this trigger
    /// </summary>
    public bool IsHovering()
    {
        return isHovering;
    }
}
