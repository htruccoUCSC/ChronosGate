using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Firemaker - Support unit that grants fire damage buffs to adjacent allies.
/// 
/// Basic mechanics:
/// - PerformBasicAttack: Applies a fire buff to all adjacent friendly units, enabling them to apply fire on their next attack
/// - CastAbility: Grants all adjacent friendly units a fire stack that will be applied on their next attack (stacks up to 5)
/// - ScanTargeting: Targets the first adjacent friendly unit
/// 
/// Based on the Wildfire augment and FirePriestess mechanics for applying fire to friendly units.
/// </summary>
public class Firemaker : BaseUnit
{
    private BoardManager m_BoardManager;
    private Buffs m_BuffSystem;

    private void Awake()
    {
        m_BoardManager = FindFirstObjectByType<BoardManager>();
        m_BuffSystem = FindFirstObjectByType<Buffs>();
    }

    protected override void ScanTargeting()
    {
        // Firemaker targets adjacent friendly units
        List<BaseUnit> adjacentAllies = GetAdjacentFriendlyUnits();
        if (adjacentAllies.Count > 0)
        {
            currentTarget = adjacentAllies[0].transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    protected override void PerformBasicAttack()
    {
        // Apply fire buff to all adjacent friendly units
        List<BaseUnit> adjacentAllies = GetAdjacentFriendlyUnits();
        
        if (adjacentAllies.Count == 0)
        {
            return;
        }

        // Apply fire to all adjacent allies
        foreach (BaseUnit ally in adjacentAllies)
        {
            ApplyFireBuff(ally);
        }

        Debug.Log($"Firemaker granted fire buff to {adjacentAllies.Count} adjacent allies");
    }

    protected override void CastAbility()
    {
        // Grant all adjacent friendly units a fire stack buff
        List<BaseUnit> adjacentAllies = GetAdjacentFriendlyUnits();
        
        if (adjacentAllies.Count == 0)
        {
            Debug.Log("Firemaker: No adjacent allies to grant fire stacks");
            return;
        }

        // Apply 5 fire stacks to all adjacent allies
        float fireStackAmount = 5f;
        foreach (BaseUnit ally in adjacentAllies)
        {
            ApplyFireStackBuff(ally, fireStackAmount);
        }

        Debug.Log($"Firemaker granted {fireStackAmount} fire stacks to {adjacentAllies.Count} adjacent allies");
    }

    /// <summary>
    /// Gets all adjacent friendly units (excluding self).
    /// </summary>
    private List<BaseUnit> GetAdjacentFriendlyUnits()
    {
        List<BaseUnit> result = new List<BaseUnit>();

        // Get boardmanager reference if we don't have it already
        if (m_BoardManager == null)
        {
            m_BoardManager = FindFirstObjectByType<BoardManager>();
            if (m_BoardManager == null || m_BoardManager.GameTilemap == null)
            {
                return result;
            }
        }

        if (m_BoardManager.unitGrid == null)
            return result;

        int gridW = m_BoardManager.unitGrid.GetLength(0);
        int gridH = m_BoardManager.unitGrid.GetLength(1);

        // Get our cell position
        Vector3Int myCell = m_BoardManager.GameTilemap.WorldToCell(transform.position);

        // 4 adjacent tiles (up, down, left, right)
        Vector2Int[] offsets =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (Vector2Int offset in offsets)
        {
            int checkX = myCell.x + offset.x;
            int checkY = myCell.y + offset.y;

            // Use the ACTUAL array size (prevents IndexOutOfRange)
            if (checkX < 0 || checkX >= gridW || checkY < 0 || checkY >= gridH)
                continue;

            BaseUnit unit = m_BoardManager.unitGrid[checkX, checkY];
            if (unit == null || unit == this || unit.myData == null)
                continue;

            result.Add(unit);
        }

        return result;
    }

    /// <summary>
    /// Applies a fire buff to a friendly unit, enabling fire application on next attack.
    /// Uses the buff system to apply fire enhancement via ApplyFire callback.
    /// </summary>
    private void ApplyFireBuff(BaseUnit targetUnit)
    {
        if (targetUnit == null || targetUnit.myData == null)
        {
            return;
        }

        // Find the Buffs system if we don't have it
        if (m_BuffSystem == null)
        {
            m_BuffSystem = FindFirstObjectByType<Buffs>();
            if (m_BuffSystem == null)
            {
                Debug.LogWarning("Firemaker: Buffs system not found");
                return;
            }
        }

        // Apply fire buff to the unit using round buff with ApplyFire callback
        m_BuffSystem.AddRoundBuff(targetUnit, 0, 0, 0, 0, 0, 0, targetUnit.ApplyFire, 1f, null, 0f);

        Debug.Log($"Firemaker gave fire buff to {targetUnit.name}");
    }

    /// <summary>
    /// Applies fire stacks to a friendly unit that will be applied on their next attack.
    /// </summary>
    private void ApplyFireStackBuff(BaseUnit targetUnit, float stackAmount)
    {
        if (targetUnit == null || targetUnit.myData == null)
        {
            return;
        }

        // Find the Buffs system if we don't have it
        if (m_BuffSystem == null)
        {
            m_BuffSystem = FindFirstObjectByType<Buffs>();
            if (m_BuffSystem == null)
            {
                Debug.LogWarning("Firemaker: Buffs system not found");
                return;
            }
        }

        // Apply fire stack buff to enable multiple fire applications on next attack
        m_BuffSystem.AddRoundBuff(targetUnit, 0, 0, 0, 0, 0, 0, targetUnit.ApplyFire, stackAmount, null, 0f);

        Debug.Log($"Firemaker gave {stackAmount} fire stacks to {targetUnit.name}");
    }
}
