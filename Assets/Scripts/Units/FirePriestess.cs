using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FirePriestess : BaseUnit
{
    [SerializeField] private CurrencyPickup m_CurrencyPickupPrefab;
    [SerializeField] private float m_CurrencyPerActiveFireBuff = 5f;
    [SerializeField] private float m_CurrencySpawnRadius = 0.4f;
    [SerializeField] private float m_FireBuffDuration = 999f; // Lasts for the rest of the round

    private BoardManager m_BoardManager;

    private void Awake()
    {
        m_BoardManager = FindFirstObjectByType<BoardManager>();
    }

    protected override void ScanTargeting()
    {
        // Fire Priestess targets friendly towers
        List<BaseUnit> adjacentTowers = GetAdjacentFriendlyTowers();
        if (adjacentTowers.Count > 0)
        {
            currentTarget = adjacentTowers[0].transform;
        }
        else
        {
            currentTarget = null;
        }
    }

    protected override void PerformBasicAttack()
    {
        // Apply fire buff to adjacent friendly towers
        List<BaseUnit> adjacentTowers = GetAdjacentFriendlyTowers();
        
        if (adjacentTowers.Count == 0)
        {
            return;
        }

        // Apply fire to one random adjacent tower
        BaseUnit targetTower = adjacentTowers[Random.Range(0, adjacentTowers.Count)];
        ApplyFireBuff(targetTower);
    }

    protected override void CastAbility()
    {
        // Count towers with active fire buff
        int towersWithFire = CountTowersWithFireBuff();

        if (towersWithFire > 0)
        {
            // Generate currency based on fire buffed towers
            int currencyAmount = Mathf.Max(1, Mathf.RoundToInt(towersWithFire * m_CurrencyPerActiveFireBuff + myData.GetModifiedAbilityPower()));
            SpawnCurrency(currencyAmount);

            Debug.Log($"Fire Priestess generated {currencyAmount} flux from {towersWithFire} fire-buffed towers");
        }
        else
        {
            Debug.Log("Fire Priestess: No towers with active fire buff");
        }
    }

    /// <summary>
    /// Gets all adjacent friendly towers.
    /// </summary>
    private List<BaseUnit> GetAdjacentFriendlyTowers()
    {
        List<BaseUnit> result = new List<BaseUnit>();

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
    /// Applies a fire buff to a friendly tower for the rest of the round.
    /// Uses the buff system to apply fire damage multiplier.
    /// </summary>
    private void ApplyFireBuff(BaseUnit targetTower)
    {
        if (targetTower == null || targetTower.myData == null)
        {
            return;
        }

        // Find the Buffs system
        Buffs buffSystem = FindFirstObjectByType<Buffs>();
        if (buffSystem == null)
        {
            Debug.LogWarning("Fire Priestess: Buffs system not found");
            return;
        }

        // Apply fire buff: increase attack damage for the rest of the round
        // Using attack damage multiplier to represent fire enhancement
        float fireBonus = 0.25f; // +25% attack damage while fire is active
        int duration = Mathf.CeilToInt(m_FireBuffDuration);
        
        buffSystem.AddTempBuff(
            targetTower,
            attackSpeedMult: 0f,
            attackSpeedFlat: 0f,
            attackDamageFlat: 0f,
            attackDamageMult: fireBonus,
            abilityPowerFlat: 0f,
            abilityPowerMult: 0f,
            duration: duration,
            OnHit: null,
            onHitModifier: 0f,
            OnKill: null,
            onKillModifier: 0f
        );

        Debug.Log($"Fire Priestess gave fire buff to {targetTower.name}");
    }

    /// <summary>
    /// Counts how many friendly towers currently have the fire buff active.
    /// </summary>
    private int CountTowersWithFireBuff()
    {
        if (m_BoardManager == null)
        {
            m_BoardManager = FindFirstObjectByType<BoardManager>();
            if (m_BoardManager == null)
            {
                return 0;
            }
        }

        int count = 0;

        if (m_BoardManager.unitList != null)
        {
            foreach (BaseUnit unit in m_BoardManager.unitList)
            {
                if (unit == null || unit == this)
                    continue;

                // Count towers that have active buffs (simplified: count any tower with active buffs)
                // In a more refined version, we could tag buffs with a specific identifier
                if (unit.activeBuffs != null && unit.activeBuffs.Count > 0)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void SpawnCurrency(int amount)
    {
        if (m_CurrencyPickupPrefab == null)
        {
            Debug.LogWarning("Fire Priestess has no currency pickup prefab assigned.");
            return;
        }

        Vector2 offset = Random.insideUnitCircle * m_CurrencySpawnRadius;
        CurrencyPickup pickup = Instantiate(m_CurrencyPickupPrefab, transform.position + (Vector3)offset, Quaternion.identity);
        pickup.Configure(amount);
    }
}

