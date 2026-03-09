using UnityEngine;

[RequireComponent(typeof(MeleeAttackBehavior))]
public class Stickman : BaseUnit
{
    [SerializeField] private bool m_KnockbackHorizontalOnly = true;
    [SerializeField] private float m_BasicAttackKnockbackDistance = 0.7f;
    [SerializeField] private float m_BasicAttackStunDuration = 0.5f;
    [SerializeField] private float m_ExtraDelayBetweenAttacks = 0.1f;
    [SerializeField] private float m_StickmanMeleeStaminaCostPerSwing = 1f;

    public override void Initialize(UnitInstance instance)
    {
        base.Initialize(instance);

        // make him a bit tankier for melee testing
        if (myData != null)
        {
            myData.CurrentHP *= 2f;
            attackTimer = 0.05f;
        }
    }

    protected override float GetAdditionalAttackDelay()
    {
        return m_ExtraDelayBetweenAttacks;
    }

    protected override float GetMeleeStaminaCostPerSwing()
    {
        return Mathf.Max(0.01f, m_StickmanMeleeStaminaCostPerSwing);
    }

    protected override void CastAbility()
    {
        Debug.Log("Stickman ability"); // perform melee attack with increased range and damage, but no knockback

        if (UsesMeleeStamina() && !HasEnoughMeleeStaminaForAttack())
        {
            return;
        }

        TryPerformMeleeAttack(myData.GetModifiedAbilityPower(), myData.BaseDef.Range + 2f);
    }

    protected override void PerformBasicAttack()
    {
        if (UsesMeleeStamina() && !HasEnoughMeleeStaminaForAttack())
        {
            return;
        }

        Debug.Log("Stickman performs basic attack");
        Transform hitTarget = currentTarget;
        float basicAttackKnockbackDistance = m_BasicAttackKnockbackDistance;
        bool didHit = TryPerformMeleeAttack(myData.GetModifiedDamage(), myData.BaseDef.Range);
        if (didHit)
        {
            ApplyBasicAttackKnockback(hitTarget, basicAttackKnockbackDistance);
            ApplyBasicAttackStun(hitTarget, m_BasicAttackStunDuration);
        }
    }

    private void ApplyBasicAttackKnockback(Transform target, float knockbackDistance)
    {
        if (target == null) return;

        Transform targetRoot = ResolveKnockbackTarget(target);
        if (targetRoot == null) return;

        Vector3 direction = targetRoot.position - transform.position;
        direction.z = 0f;

        if (m_KnockbackHorizontalOnly)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = Vector3.right;
        }

        float finalKnockbackDistance = Mathf.Max(0f, knockbackDistance);
        targetRoot.position += direction.normalized * finalKnockbackDistance;
    }

    private void ApplyBasicAttackStun(Transform target, float stunDuration)
    {
        if (target == null) return;
        if (stunDuration <= 0f) return;

        BaseEnemy enemy = target.GetComponentInParent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.ApplyStun(stunDuration);
        }
    }

    private Transform ResolveKnockbackTarget(Transform target)
    {
        BaseEnemy enemy = target.GetComponentInParent<BaseEnemy>();
        if (enemy != null)
        {
            return enemy.transform;
        }

        TargetDummyTest dummy = target.GetComponentInParent<TargetDummyTest>();
        if (dummy != null)
        {
            return dummy.transform;
        }

        return target;
    }
}
