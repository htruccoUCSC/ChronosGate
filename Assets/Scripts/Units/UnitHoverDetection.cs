using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach this to placed unit GameObjects to enable tooltip display on hover
/// Works with 2D colliders using Physics2D raycasting
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class UnitHoverDetection : MonoBehaviour
{
    private bool isHovering = false;
    private BaseUnit baseUnit;
    private Collider2D col2D;
    
    private static UnitHoverDetection currentHoveredUnit = null;

    private void Awake()
    {
        baseUnit = GetComponent<BaseUnit>();
        col2D = GetComponent<Collider2D>();
        
        if (baseUnit == null)
        {
            Debug.LogWarning($"[UnitHoverDetection] No BaseUnit component found on {gameObject.name}. Tooltip will not work.");
        }

        if (col2D == null)
        {
            Debug.LogWarning($"[UnitHoverDetection] No Collider2D found on {gameObject.name}. Tooltip will not work.");
        }
    }

    private void Update()
    {
        if (baseUnit == null || col2D == null || Mouse.current == null) return;

        // Get mouse position in world space
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;

        // Check if mouse is over this unit's collider
        bool isMouseOver = col2D.OverlapPoint(worldPosition);

        if (isMouseOver && !isHovering)
        {
            // Mouse entered
            OnMouseEnter();
        }
        else if (!isMouseOver && isHovering)
        {
            // Mouse exited
            OnMouseExit();
        }
    }

    private void OnMouseEnter()
    {
        // If another unit is already hovered, tell it to exit first
        if (currentHoveredUnit != null && currentHoveredUnit != this)
        {
            currentHoveredUnit.OnMouseExit();
        }

        isHovering = true;
        currentHoveredUnit = this;

        if (baseUnit.myData != null && TowerTooltipUI.Instance != null)
        {
            TowerTooltipUI.Instance.ShowTooltip(baseUnit.myData);
        }
    }

    private void OnMouseExit()
    {
        if (!isHovering) return;

        isHovering = false;
        
        if (currentHoveredUnit == this)
        {
            currentHoveredUnit = null;
        }

        if (TowerTooltipUI.Instance != null)
        {
            TowerTooltipUI.Instance.HideTooltip();
        }
    }

    private void OnDisable()
    {
        if (isHovering)
        {
            OnMouseExit();
        }
    }

    private void OnDestroy()
    {
        if (isHovering && TowerTooltipUI.Instance != null)
        {
            TowerTooltipUI.Instance.HideTooltip();
        }
    }
}
