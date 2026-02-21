using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeleeAttackBehavior))]
public class RiotControl : BaseUnit
{
    private float m_currenthealth;
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private bool m_KnockbackHorizontalOnly = true;
    protected override void CastAbility()
    {
        Debug.Log("RiotControl ability");//apply knockback to all enemies in one tile?
        float abilityKnockbackDistance = 3f;
        LayerMask mask = m_TargetMask.value == 0 
            ? LayerMask.GetMask("Enemies") 
            : m_TargetMask;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, mask);
        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.CompareTag("Enemy")) 
                continue;

            TargetDummyTest enemy = hit.GetComponentInParent<TargetDummyTest>();
            if (enemy == null) 
                continue;

            ApplyBasicAttackKnockback(enemy.transform, abilityKnockbackDistance);
        }
    
    }
    
    protected override void PerformBasicAttack()
    {
        Debug.Log("RiotControl performs basic attack");
        TryPerformMeleeAttack(myData.GetModifiedDamage(), myData.BaseDef.Range);
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

    //ADD TakeDamage(int amount, Transform attacker)) to enemy script
    //Keep track of attacker to apply thorns and other reactive effects
    // public override void TakeDamage(int amount, Transform attacker = null)
    // {
    //     // Apply damage to this unit
    //     m_currenthealth -= amount;
    //     Debug.Log($"{gameObject.name} took {amount} damage from {attacker?.name ?? "unknown"}");
    //     thornsAmount = myData.BaseDef.AttackDamage / 2; //Example thorns dmg probably needs adjustment 
    //     if (attacker != null)
    //     {
    //         attacker.health -= thornsAmount;
    //         Debug.Log($"{attacker.name} takes {thornsAmount} thorns damage");
    //     }
    //     if (m_currenthealth <= 0)
    //     {
    //         Destroy(gameObject);
    //         Debug.Log($"{gameObject.name} has been destroyed");
    //     }
    // }

}

    
