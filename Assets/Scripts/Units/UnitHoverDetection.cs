using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Right-click a placed unit to open/close the detail tooltip.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class UnitHoverDetection : MonoBehaviour
{
    private BaseUnit baseUnit;
    private Collider2D col2D;

    // Track which unit currently has the tooltip open so a second right-click
    // on anything (or a right-click on empty space) can close it.
    private static UnitHoverDetection s_OpenUnit = null;

    private void Awake()
    {
        baseUnit = GetComponent<BaseUnit>();
        col2D = GetComponent<Collider2D>();

        if (baseUnit == null)
            Debug.LogWarning($"[UnitHoverDetection] No BaseUnit on {gameObject.name}.");
        if (col2D == null)
            Debug.LogWarning($"[UnitHoverDetection] No Collider2D on {gameObject.name}.");
    }

    private void Update()
    {
        if (baseUnit == null || col2D == null || Mouse.current == null) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
            world.z = 0f;

            bool overThis = col2D.OverlapPoint(world);

            if (overThis)
            {
                // Toggle: if this unit is already open, close it; otherwise open it.
                if (s_OpenUnit == this)
                    CloseTooltip();
                else
                    OpenTooltip();
            }
            else if (s_OpenUnit == this)
            {
                // Right-clicked somewhere else while this tooltip is open — close.
                CloseTooltip();
            }
        }
    }

    private void OpenTooltip()
    {
        // Close whatever was previously open
        if (s_OpenUnit != null && s_OpenUnit != this)
            s_OpenUnit.CloseTooltip();

        s_OpenUnit = this;

        if (baseUnit.myData != null && TowerTooltipUI.Instance != null)
            TowerTooltipUI.Instance.ShowTooltip(baseUnit.myData);
    }

    private void CloseTooltip()
    {
        if (s_OpenUnit == this)
            s_OpenUnit = null;

        if (TowerTooltipUI.Instance != null)
            TowerTooltipUI.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (s_OpenUnit == this)
            CloseTooltip();
    }

    private void OnDestroy()
    {
        if (s_OpenUnit == this)
            CloseTooltip();
    }
}
