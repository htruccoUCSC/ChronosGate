using System.Collections.Generic;
using UnityEngine;

public class SeerUnit : BaseUnit
{
    [SerializeField] private float m_BasicManaGive = 15f;
    [SerializeField] private int m_AbilityCopies = 1;

    private BoardManager m_BoardManager;
    private InventoryUI m_InventoryUI;

    private void Awake()
    {
        m_BoardManager = FindFirstObjectByType<BoardManager>();
        m_InventoryUI = FindFirstObjectByType<InventoryUI>();
    }

    protected override void ScanTargeting()
    {
        // Support unit: keep the attack loop active without enemy targeting.
        currentTarget = transform;
    }

    protected override void PerformBasicAttack()
    {
        BaseUnit ally = GetRandomFriendlyAlly();
        if (ally == null || ally.myData == null || ally.myData.BaseDef == null)
        {
            // Debug signal for test runs where Seer has no valid ally to support.
            Debug.Log("Seer basic attack skipped: no valid ally found.");
            return;
        }

        float manaToGive = Mathf.Max(0f, m_BasicManaGive);
        float abilityManaCost = Mathf.Max(0f, ally.myData.BaseDef.AbilityManaCost);

        if (abilityManaCost > 0f)
        {
            ally.myData.CurrentMana = Mathf.Min(ally.myData.CurrentMana + manaToGive, abilityManaCost);
        }
        else
        {
            ally.myData.CurrentMana += manaToGive;
        }

        // Debug signal for mana support behavior.
        Debug.Log($"Seer gave {manaToGive} mana to {ally.name}.");
    }

    protected override void CastAbility()
    {
        if (m_InventoryUI == null)
        {
            m_InventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (m_InventoryUI == null)
        {
            Debug.LogWarning("Seer could not duplicate a unit because InventoryUI is missing.");
            return;
        }

        BaseUnit ally = GetRandomFriendlyAlly();
        if (ally == null || ally.myData == null || ally.myData.BaseDef == null)
        {
            // Debug signal for cast attempts with no valid unit to duplicate.
            Debug.Log("Seer ability skipped: no valid ally found to duplicate.");
            return;
        }

        UnitDefinition copyDef = ally.myData.BaseDef;
        int copiesToAdd = Mathf.Max(1, m_AbilityCopies);
        int added = 0;

        for (int i = 0; i < copiesToAdd; i++)
        {
            if (m_InventoryUI.AddUnit(copyDef))
            {
                added++;
            }
            else
            {
                break;
            }
        }

        // Debug signal for duplicate/deck-injection behavior.
        Debug.Log($"Seer duplicated {copyDef.Name} x{added}.");
    }

    private BaseUnit GetRandomFriendlyAlly()
    {
        if (m_BoardManager == null)
        {
            m_BoardManager = FindFirstObjectByType<BoardManager>();
        }

        if (m_BoardManager == null || m_BoardManager.unitList == null)
        {
            return null;
        }

        List<BaseUnit> candidates = new List<BaseUnit>();

        for (int i = 0; i < m_BoardManager.unitList.Count; i++)
        {
            BaseUnit unit = m_BoardManager.unitList[i];
            if (unit == null || unit == this || unit.myData == null || unit.IsDead)
            {
                continue;
            }

            candidates.Add(unit);
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }
}
