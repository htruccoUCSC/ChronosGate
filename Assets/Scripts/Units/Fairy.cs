using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Fairy : BaseUnit
{
    // [SerializeField] private CurrencyPickup m_CurrencyPickupPrefab;

    [SerializeField] private float m_HealAmount = 15f;
    [SerializeField] private float m_OverhealCurrencyMultiplier = 1.5f;

    private BoardManager m_BoardManager;

    private void Awake()
    {
        m_BoardManager = FindFirstObjectByType<BoardManager>();
    }

    protected override void ScanTargeting()
    {
        BaseUnit lowestHealthTower = GetLowestHealthFriendlyTower();

        if (lowestHealthTower != null)
            currentTarget = lowestHealthTower.transform;
        else
            currentTarget = null;
    }

    protected override void PerformBasicAttack()
    {
        // Generate currency every other basic attack (double the time required)

            SpawnCurrency((int)(myData.GetModifiedDamage() / 50f));

    }

    protected override void CastAbility()
    {
        // Heal the lowest health tower and gain currency for overheal
        BaseUnit targetUnit = GetLowestHealthFriendlyTower();

        if (targetUnit == null)
        {
            Debug.LogWarning("Fairy: No friendly towers found for healing");
            return;
        }

        // Heal the target
        float healAmount = myData.GetModifiedAbilityPower() + m_HealAmount;
        float maxHP = targetUnit.myData.BaseDef.Health;

        targetUnit.myData.CurrentHP += healAmount;

        // Calculate overheal and generate currency
        float overhealAmount = 0f;
        if (targetUnit.myData.CurrentHP > maxHP)
        {
            overhealAmount = targetUnit.myData.CurrentHP - maxHP;
            targetUnit.myData.CurrentHP = maxHP;
        }

        // Generate currency for overheal
        if (overhealAmount > 0f)
        {
            int overhealCurrency = Mathf.Max(1, Mathf.RoundToInt(overhealAmount * m_OverhealCurrencyMultiplier));
            SpawnCurrency(overhealCurrency);
        }

        // Also generate base currency
        SpawnCurrency((int)(myData.GetModifiedDamage() / 50f));

        Debug.Log($"Fairy healed {targetUnit.name} for {healAmount}. Overheal: {overhealAmount}");
    }

    /// <summary>
    /// Finds the friendly tower with the lowest health on the board.
    /// </summary>
    private BaseUnit GetLowestHealthFriendlyTower()
    {
        if (m_BoardManager == null)
        {
            m_BoardManager = FindFirstObjectByType<BoardManager>();
            if (m_BoardManager == null || m_BoardManager.unitGrid == null)
            {
                return null;
            }
        }

        BaseUnit lowestHealthTower = null;
        float lowestHealth = float.MaxValue;

        int gridW = m_BoardManager.unitGrid.GetLength(0);
        int gridH = m_BoardManager.unitGrid.GetLength(1);

        for (int x = 0; x < gridW; x++)
        {
            for (int y = 0; y < gridH; y++)
            {
                BaseUnit unit = m_BoardManager.unitGrid[x, y];

                if (unit == null || unit == this || unit.myData == null)
                    continue;

                // Find the tower with the lowest health
                if (unit.myData.CurrentHP < lowestHealth)
                {
                    lowestHealth = unit.myData.CurrentHP;
                    lowestHealthTower = unit;
                }
            }
        }

        return lowestHealthTower;
    }

    private void SpawnCurrency(int amount)
    {
        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("Fairy could not add currency because CurrencyManager is missing.");
            return;
        }

        CurrencyManager.Instance.AddCurrency(amount, transform.position);
    }
}
