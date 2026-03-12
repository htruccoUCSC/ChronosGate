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
        // Re-ensure the round buff in case round cleanup removed it.
        EnsureFireOnHitBuff();
        Debug.Log("Dragon basic attack fired (applies fire on hit).");
        SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
    }

    protected override void CastAbility()
    {
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        float burnStacks = Mathf.Max(1f, m_AbilityBurnStacks);

        if (enemies.Length == 0)
        {
            // Debug signal to show cast happened even when no enemies were available.
            Debug.Log("Dragon ability cast: no enemies found on board.");
        }

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

        // Debug signal for board-wide burn application.
        Debug.Log($"Dragon applied burn to {enemies.Length} enemies.");
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

        // Debug signal that fire-on-hit round buff is active on Dragon.
        Debug.Log($"Dragon fire-on-hit buff set to {m_FireOnHitBuff.OnhitModifier} stack(s).");
        AddRoundBuff(m_FireOnHitBuff);
    }
}
