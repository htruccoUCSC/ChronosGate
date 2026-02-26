using UnityEngine;
using System.Collections.Generic;

public class Hero : BaseUnit
{
    [Header("Combat Settings")]
    [SerializeField] private float meleeRange = 1.25f;
    
    [Header("Ability Settings")]
    [SerializeField] private float abilityBuffDuration = 5f; // Duration of attack speed buff in seconds
    [SerializeField] private float abilityAttackSpeedBoost = 0.2f; // 20% attack speed boost
    
    private MeleeAttackBehavior meleeAttackBehavior;
    private bool isUsingMelee = false;
    private BoardManager boardManager;
    private Buffs buffSystem;

    private void Awake()
    {
        
        meleeAttackBehavior = GetComponent<MeleeAttackBehavior>();
        if (meleeAttackBehavior == null)
        {
            Debug.LogWarning("[Hero] No MeleeAttackBehavior component found.");
        }

        // Get references to systems
        boardManager = FindFirstObjectByType<BoardManager>();
        buffSystem = FindFirstObjectByType<Buffs>();
    }

    protected override void PerformBasicAttack()
    {
        if (currentTarget == null) return;

        float damage = myData.GetModifiedDamage();
        
        // Determine if we should use melee or ranged based on distance to target
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        isUsingMelee = distanceToTarget <= meleeRange;
        
        
        if (isUsingMelee)
        {
            PerformMeleeAttack(damage * 1.25f);
        }
        else
        {
            PerformRangedAttack(damage);
        }
    }

    private void PerformMeleeAttack(float damage)
    {
        if (meleeAttackBehavior == null)
        {
            Debug.LogWarning("[Hero] Cannot perform melee attack - no MeleeAttackBehavior!");
            return;
        }
        
        bool didHit = TryPerformMeleeAttack(damage, meleeRange);
        
    }

    private void PerformRangedAttack(float damage)
    {
        if (currentTarget == null) return;
        
        GameObject projectilePrefab = LoadProjectilePrefab();
        if (projectilePrefab == null)
        {
            return;
        }

        SpawnProjectile(projectilePrefab, damage, false);
    }

    protected override void CastAbility()
    {
        
        // Get all units in the same column
        List<BaseUnit> columnUnits = GetUnitsInColumn();
        
        if (columnUnits.Count == 0)
        {
            Debug.Log("[Hero] No units in column to buff!");
            return;
        }

        // Apply attack speed buff to each unit
        foreach (BaseUnit unit in columnUnits)
        {
            if (buffSystem != null)
            {
                // Use the Buffs system: AddTempBuff(unit, attackSpeedMult, attackSpeedFlat, attackDamageFlat, attackDamageMult, abilityPowerFlat, abilityPowerMult, duration, OnHit, onHitModifier, OnKill, onKillModifier)
                buffSystem.AddTempBuff(
                    unit, 
                    abilityAttackSpeedBoost, // 20% attack speed multiplier
                    0f, // no flat attack speed
                    0f, // no flat attack damage
                    0f, // no attack damage multiplier
                    0f, // no flat ability power
                    0f, // no ability power multiplier
                    Mathf.CeilToInt(abilityBuffDuration), // duration in seconds
                    null, // no OnHit action
                    0f, // no OnHit modifier
                    null, // no OnKill action
                    0f  // no OnKill modifier
                );
                Debug.Log($"[Hero] Buffed {unit.name} with {abilityAttackSpeedBoost * 100}% attack speed for {abilityBuffDuration} seconds!");
            }
            else
            {
                // Fallback: create buff manually
                Buff attackSpeedBuff = new Buff 
                { 
                    AttackSpeedMult = abilityAttackSpeedBoost, 
                    duration = abilityBuffDuration 
                };
                unit.AddTempBuff(attackSpeedBuff);
                Debug.Log($"[Hero] Buffed {unit.name} (fallback method) with {abilityAttackSpeedBoost * 100}% attack speed!");
            }
        }
        
        Debug.Log($"[Hero] Buffed {columnUnits.Count} units in column!");
    }

    private List<BaseUnit> GetUnitsInColumn()
    {
        List<BaseUnit> result = new List<BaseUnit>();

        // Check if we have board manager
        if (boardManager == null)
        {
            boardManager = FindFirstObjectByType<BoardManager>();
            if (boardManager == null || boardManager.GameTilemap == null)
            {
                Debug.LogWarning("[Hero] BoardManager not found!");
                return result;
            }
        }

        if (boardManager.unitGrid == null)
        {
            Debug.LogWarning("[Hero] Unit grid not initialized!");
            return result;
        }

        // Get our cell position
        Vector3Int myCell = boardManager.GameTilemap.WorldToCell(transform.position);
        int myColumn = myCell.x;

        // Get grid dimensions
        int gridHeight = boardManager.unitGrid.GetLength(1);

        // Search through all rows in our column
        for (int y = 0; y < gridHeight; y++)
        {
            // Make sure we're within bounds
            if (myColumn < 0 || myColumn >= boardManager.unitGrid.GetLength(0))
                continue;

            BaseUnit unit = boardManager.unitGrid[myColumn, y];
            
            // Skip null units, self, and units without data
            if (unit == null || unit == this || unit.myData == null)
                continue;

            result.Add(unit);
        }

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        if (myData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, myData.BaseDef.Range);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        if (currentTarget != null)
        {
            Gizmos.color = isUsingMelee ? Color.red : Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
