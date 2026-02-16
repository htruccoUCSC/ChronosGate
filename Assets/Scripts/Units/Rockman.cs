using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class RockmanUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;


    protected override void CastAbility()
    {
        Debug.Log("Rock-man uses ability");
        SpawnRockmanAbilityProjectile(myData.GetModifiedDamage() * 2f);
    }

    protected override void PerformBasicAttack()
    {
        SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
    }

    private void SpawnRockmanAbilityProjectile(float damage)
    {
        GameObject projectilePrefab = LoadProjectilePrefab();
        if (currentTarget == null || projectilePrefab == null) return;

        GameObject proj = InstantiateAndSetupProjectile(projectilePrefab);
        if (proj == null) return;

        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return;

        Vector2 diff = currentTarget.position - transform.position;
        float distance = diff.magnitude;
        Vector2 direction = diff.normalized;
        float launchAngle = myData.BaseDef.LaunchAngle;

        if (launchAngle > 0)
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);
            projScript.speed = CalculateBallisticSpeed(distance, launchAngle, gravity);
        }

        projScript.Setup(damage, direction, launchAngle, transform.position, true);
        projScript.EnableOnHitSlow(0.30f, 3f);
    }

}

