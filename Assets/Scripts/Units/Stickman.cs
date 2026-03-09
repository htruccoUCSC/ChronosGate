using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeleeAttackBehavior))]
public class Stickman : BaseUnit
{
    [SerializeField] private bool m_KnockbackHorizontalOnly = true;
    protected override void CastAbility()
    {
        Debug.Log("Stickman ability");//perform melee attack with increased range and damage, but no knockback
        TryPerformMeleeAttack(myData.GetModifiedAbilityPower(), myData.GetModifiedRange() + 2f);
    }
    
    protected override void PerformBasicAttack()
    {
        Debug.Log("Stickman performs basic attack");
        Transform hitTarget = currentTarget;
        float basicAttackKnockbackDistance = 0.75f;
        bool didHit = TryPerformMeleeAttack(myData.GetModifiedDamage(), myData.GetModifiedRange());
        if (didHit)
        {
            ApplyBasicAttackKnockback(hitTarget, basicAttackKnockbackDistance);
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

    private Transform ResolveKnockbackTarget(Transform target)
    {
        TargetDummyTest dummy = target.GetComponentInParent<TargetDummyTest>();
        if (dummy != null)
        {
            return dummy.transform;
        }

        return target;
    }
}

