using UnityEngine;

public class UnicornUnit : BaseUnit
{
    // Basic heal is the "auto attack" heal.
    [SerializeField] private float basicHealAmount = 1f;
    // Ability heal is stronger and happens when mana is full.
    [SerializeField] private float abilityHealAmount = 3f;

    private BoardManager m_BoardManager;

    public override void Initialize(UnitInstance instance)
    {
        base.Initialize(instance);
        m_BoardManager = FindFirstObjectByType<BoardManager>();
        if (m_BoardManager == null)
            Debug.LogError("BoardManager not found for UnicornUnit.");
    }

    protected override void ScanTargeting()
    {
        currentTarget = transform;
    }

    protected override void PerformBasicAttack()
    {
        BaseUnit ally = GetLowestHealthPercentAlly();
        if (ally == null) return;

        Debug.Log($"Unicorn basic heal on {ally.name} for {basicHealAmount}.");
        HealUnit(ally, basicHealAmount);
    }

    protected override void CastAbility()
    {
        // Future improvement: add a separate ability cooldown timer so
        // basic-heal cadence and ability-heal cadence can be tuned independently.
        BaseUnit ally = GetLowestHealthPercentAlly();
        if (ally == null) return;

        Debug.Log($"Unicorn ability heal on {ally.name} for {abilityHealAmount}.");
        HealUnit(ally, abilityHealAmount);
        Debug.Log("Unicorn uses ability: big heal.");
    }

    private BaseUnit GetLowestHealthPercentAlly()
    {
        if (m_BoardManager == null)
            m_BoardManager = FindFirstObjectByType<BoardManager>();

        if (m_BoardManager == null) return null;

        BaseUnit best = null;
        float bestPct = float.MaxValue;

        foreach (var u in m_BoardManager.unitList)
        {
            // Skip invalid, dead, or self targets.
            if (u == null || u == this || u.myData == null || u.IsDead) continue;

            float maxHp = u.myData.MaxHP;
            if (maxHp <= 0f) continue;

            // We heal by percent so tanks and squishy units are healed more fairly.
            float pct = u.myData.CurrentHP / maxHp;
            if (pct >= 0.999f) continue;

            if (pct < bestPct)
            {
                bestPct = pct;
                best = u;
            }
        }

        return best;
    }
    private void HealUnit(BaseUnit target, float healAmount)
    {
        if (target == null || target.myData == null) return;

        // Clamp so healing never goes over max HP.
        target.myData.CurrentHP = Mathf.Min(
            target.myData.MaxHP,
            target.myData.CurrentHP + healAmount
        );
    }
}