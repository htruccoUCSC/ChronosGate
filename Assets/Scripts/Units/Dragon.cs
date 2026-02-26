using UnityEngine;

public class DragonUnit : BaseUnit
{
    [SerializeField] private float m_FireStacksOnHit = 1f;
    [SerializeField] private float m_AbilityBurnStacks = 3f;

    private Buff m_FireOnHitBuff;

    public override void Initialize(UnitInstance instance)
    {
        base.Initialize(instance);
        EnsureFireOnHitBuff();
    }

    protected override void PerformBasicAttack()
    {
        // If round buffs were cleared, restore the dragon's fire-on-hit behavior.
        EnsureFireOnHitBuff();
        SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
    }

    protected override void CastAbility()
    {
        float burnStacks = Mathf.Max(1f, m_AbilityBurnStacks);
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);

        for (int i = 0; i < enemies.Length; i++)
        {
            BaseEnemy enemy = enemies[i];
            if (enemy == null) continue;

            enemy.ApplyDebuff(
                BaseEnemy.DebuffType.Burn,
                burnStacks,
                DebuffDuration.BurnDuration,
                enemy.ApplyFire
            );
        }

        Debug.Log($"Dragon ability applies burn to {enemies.Length} enemies.");
    }

    private void EnsureFireOnHitBuff()
    {
        bool needsBuff = m_FireOnHitBuff == null || !roundBuffs.Contains(m_FireOnHitBuff);
        if (!needsBuff) return;

        m_FireOnHitBuff = new Buff
        {
            OnHit = ApplyFire,
            OnhitModifier = Mathf.Max(1f, m_FireStacksOnHit)
        };

        AddRoundBuff(m_FireOnHitBuff);
    }
}
