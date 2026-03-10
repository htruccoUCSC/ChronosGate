using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ElectricKatana - Melee/Piercing unit that fires a spark projectile through enemies.
/// 
/// Basic mechanics:
/// - PerformBasicAttack: Launches a spark projectile that passes through the melee-range target and hits the next enemy in the row
/// - CastAbility: Performs AOE damage to all visible enemies on screen
/// - ScanTargeting: Uses inherited raycast to find forward enemies
/// 
/// The projectile has a max penetration of 2, allowing it to pass through the first target and hit the next enemy, then stop.
/// </summary>
public class ElectricKatana : BaseUnit
{
    [SerializeField] private float m_SparkProjectileScale = 0.6f;

    protected override void PerformBasicAttack()
    {
        // Launch the electric spark projectile
        SpawnSparkProjectile(myData.GetModifiedDamage());
    }

    protected override void CastAbility()
    {
        // Perform AOE damage to all enemies currently visible on screen
        PerformScreenWideAttack(myData.GetModifiedDamage() + myData.GetModifiedAbilityPower());
    }

    /// <summary>
    /// Spawns a spark projectile that passes through the melee-range enemy and hits the next one in the row.
    /// The projectile travels forward in a straight line at high speed without gravity.
    /// </summary>
    private void SpawnSparkProjectile(float damage)
    {
        if (currentTarget == null) return;

        GameObject projectilePrefab = LoadProjectilePrefab();
        if (projectilePrefab == null) return;

        GameObject proj = InstantiateAndSetupProjectile(projectilePrefab);
        if (proj == null) return;

        Projectile p = proj.GetComponentInChildren<Projectile>();
        if (p == null) return;

        // Force straight horizontal travel without arc
        p.Setup(damage, Vector2.right, 0f, transform.position, false, this);
        
        // Enable pass-through so projectile continues after hitting the first enemy
        p.passThroughEnemies = true;
        
        // Stop after hitting 2 enemies (pass through first, hit second, then stop)
        p.maxPenetration = 2;

        // Scale down the projectile spark
        if (m_SparkProjectileScale != 1f)
        {
            p.transform.localScale *= m_SparkProjectileScale;
        }

        Debug.Log("ElectricKatana launched spark projectile through melee target");
    }

    /// <summary>
    /// Performs screen-wide AOE attack against all visible enemies.
    /// This is the ability: damage applied to all enemies on screen.
    /// </summary>
    private void PerformScreenWideAttack(float damage)
    {
        LayerMask enemyMask = LayerMask.GetMask("Enemies");
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogWarning("ElectricKatana: No main camera found");
            return;
        }

        // Get the screen bounds in world coordinates
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector2(1, 1));
        Vector2 center = (bottomLeft + topRight) / 2f;
        Vector2 size = topRight - bottomLeft;

        // Find all enemies within the screen area
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, enemyMask);
        int enemiesHit = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null) continue;

            // Apply damage to the enemy
            int damageAmount = Mathf.RoundToInt(damage);
            enemy.TakeDamage(this, damageAmount);
            enemiesHit++;
        }

        Debug.Log($"ElectricKatana ability hit {enemiesHit} enemies with {damage} damage");
    }
}
