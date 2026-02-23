using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeleeAttackBehavior))]
public class Orc : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private float m_buffDuration = 5f;

    private float m_AbilityProjectileScaleMultiplier = 5f;

    protected override void CastAbility()
    {
        // Add attack speed buff on ability cast
        Buff attackSpeedBuff = new Buff {
            AttackSpeedMult = 1.5f, // +50% attack speed
            duration = m_buffDuration
        };
        AddTempBuff(attackSpeedBuff);
        Debug.Log("Orc uses ability and gains attack speed buff");
        myData.CurrentMana = 0f;
    }

    protected override void PerformBasicAttack()
    {
        if(!TryPerformMeleeAttack(myData.GetModifiedDamage(), 1f))
        {
            SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
        }
        
    }
    
    private void SpawnOrcAbilityProjectile(float damage)
    {
        if (currentTarget == null || m_ProjectilePrefab == null) return;

        GameObject proj = Instantiate(m_ProjectilePrefab, transform.position, Quaternion.identity);

        if (_projectileSprite != null)
        {
            var sr = proj.GetComponentInChildren<SpriteRenderer>();
            var animator = proj.GetComponentInChildren<Animator>();
            if (sr != null && animator == null) sr.sprite = _projectileSprite;
            proj.transform.localScale = Vector3.Scale(transform.localScale, _projectileScale) * m_AbilityProjectileScaleMultiplier;
        }

        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return;

        Vector2 diff = currentTarget.position - transform.position;
        Vector2 direction = diff.normalized;
        float launchAngle = myData.BaseDef.LaunchAngle;

        if (launchAngle > 0)
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);
            projScript.speed = CalculateBallisticSpeed(diff, launchAngle, gravity);
        }

        projScript.Setup(damage, direction, launchAngle, transform.position, true, this);
        projScript.EnableOnHitSlow(0.30f, 3f);
    }
}

