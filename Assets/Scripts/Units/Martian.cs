using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Martian:BaseUnit
{
    protected override void PerformBasicAttack()
    {
        if (currentTarget == null) return;

        GameObject prefab = LoadProjectilePrefab();
        if (prefab == null) return;

        GameObject projGO = InstantiateAndSetupProjectile(prefab);
        if (projGO == null) return;

        projGO.transform.localScale *= 0.03f;

        Projectile proj = projGO.GetComponentInChildren<Projectile>();
        if (proj == null) return;

        Vector2 diff = currentTarget.position - transform.position;
        Vector2 direction = diff.normalized;
        float launchAngle = myData.BaseDef.LaunchAngle;

        if (launchAngle > 0)
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);
            proj.speed = CalculateBallisticSpeed(diff, launchAngle, gravity);
        }
        proj.Setup(myData.GetModifiedDamage(), direction, launchAngle, transform.position, false, this);
        proj.EnableOnHitSlow(0.5f, 2f); // 50% slow for 2 seconds
    }
        override protected void CastAbility()
    {
        Debug.Log("Martian uses ability");
        if (currentTarget == null) return;

        float threshold = 0.3f;
        var enemies = Object.FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        var target = System.Linq.Enumerable.FirstOrDefault(
            enemies,
            e => e.HealthPercent <= threshold && e.currentHealth > 0
        );
        
        // Check for execute condition
        if (target != null){
            target.TakeDamage(this,target.currentHealth); // Execute the enemy
            Debug.Log("Martian executes " + target.name);
        }
    }
}



