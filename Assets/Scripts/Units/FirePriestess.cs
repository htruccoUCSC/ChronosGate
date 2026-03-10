using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class FirePriestess : BaseUnit
{
    [SerializeField] private CurrencyPickup m_CurrencyPickupPrefab;
    [SerializeField] private float m_CurrencyPerActiveFireBuff = 5f;
    [SerializeField] private float m_CurrencySpawnRadius = 0.4f;

    private BoardManager m_BoardManager;
    private Buffs m_BuffSystem;

    private void Awake()
    {
        m_BoardManager = FindFirstObjectByType<BoardManager>();
        m_BuffSystem = FindFirstObjectByType<Buffs>();
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
        // Apply fire buff to adjacent friendly towers that don't already have one
        List<BaseUnit> adjacentTowers = GetAdjacentFriendlyTowers();
        
        // Filter out towers that already have the fire buff
        List<BaseUnit> availableTowers = new List<BaseUnit>();
        foreach (BaseUnit tower in adjacentTowers)
        {
            if (tower.roundBuffs == null || tower.roundBuffs.Count == 0)
            {
                availableTowers.Add(tower);
            }
        }

        if (availableTowers.Count == 0)
        {
            return;
        }

        // Apply fire to one random available tower
        BaseUnit targetTower = availableTowers[Random.Range(0, availableTowers.Count)];
        ApplyFireBuff(targetTower);
    }

    protected override void CastAbility()
    {
        // Count total fire stacks on all enemies
        int totalFireStacks = CountTotalEnemyFireStacks();

        if (totalFireStacks > 0)
        {
            // Generate currency based on fire stacks
            int currencyAmount = Mathf.Max(1, Mathf.RoundToInt(totalFireStacks * m_CurrencyPerActiveFireBuff + myData.GetModifiedAbilityPower()));
            SpawnCurrency(currencyAmount);

            Debug.Log($"Fire Priestess generated {currencyAmount} flux from {totalFireStacks} fire stacks on enemies");
        }
        else
        {
            Debug.Log("Fire Priestess: No fire stacks on enemies");
        }
    }

    /// <summary>
    /// Gets all adjacent friendly towers.
    /// </summary>
    private List<BaseUnit> GetAdjacentFriendlyTowers()
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
    /// Applies a fire buff to a friendly tower for the rest of the round.
    /// Uses the buff system to apply fire enhancement via ApplyFire callback.
    /// </summary>
    private void ApplyFireBuff(BaseUnit targetTower)
    {
        if (targetTower == null || targetTower.myData == null)
        {
            return;
        }

        // Find the Buffs system if we don't have it
        if (m_BuffSystem == null)
        {
            m_BuffSystem = FindFirstObjectByType<Buffs>();
            if (m_BuffSystem == null)
            {
                Debug.LogWarning("Fire Priestess: Buffs system not found");
                return;
            }
        }

        // Apply fire buff to the tower using round buff with ApplyFire callback
        m_BuffSystem.AddRoundBuff(targetTower, 0, 0, 0, 0, 0, 0, 0f, targetTower.ApplyFire, 1f, null, 0f);

        Debug.Log($"Fire Priestess gave fire buff to {targetTower.name}");
    }

    /// <summary>
    /// Counts the total number of fire stacks across all enemies on the board.
    /// </summary>
    private int CountTotalEnemyFireStacks()
    {
        int totalStacks = 0;

        LayerMask mask = LayerMask.GetMask("Enemies");
        Camera cam = Camera.main;

        if (cam == null) return 0;

        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector2(1, 1));

        Vector2 center = (bottomLeft + topRight) / 2f;
        Vector2 size = topRight - bottomLeft;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, mask);

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null) continue;

            // Check if enemy has fire debuff
            if (enemy.Debuffs.TryGetValue(BaseEnemy.DebuffType.Burn, out Debuff burnDebuff))
            {
                totalStacks += Mathf.RoundToInt(burnDebuff.amountOfStacks);
            }
        }

        return totalStacks;
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

