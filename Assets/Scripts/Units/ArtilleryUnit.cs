using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Artillery (Bomber) - Fires explosive projectiles and increases fire rate on ability.
/// 
/// Basic mechanics:
/// - PerformBasicAttack: Fires an AOE (explosive) projectile at the nearest target
/// - CastAbility: Grants +20% attack speed (crew loads shells 20% faster) for the rest of the round
/// - ScanTargeting: Uses GetNearestTargets() inherited from BaseUnit to target closest enemy
/// 
/// Uses existing BaseUnit methods:
/// - SpawnProjectile(prefab, damage, isAOE): Handles projectile creation and setup
/// - AddRoundBuff(buff): Applies buff that lasts the entire round
/// - CalculateBallisticSpeed(): Calculates projectile launch velocity (used by SpawnProjectile)
/// </summary>
public class ArtilleryUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private float m_CrewLoadSpeedBoost = 0.20f; // 20% attack speed increase
    private Vector3 offsetTargetPosition; // Position 1 tile to the right of the target

    protected override void CastAbility()
    {
        // Create a round-long buff that increases attack speed by 20%.
        // This makes the unit fire 20% faster (shorter cooldown between attacks).
        Buff crewLoadBuff = new Buff
        {
            AttackSpeedFlat = myData.GetModifiedAttackSpeed() * m_CrewLoadSpeedBoost,
            duration = float.MaxValue // Lasts for entire round
        };
        
        AddRoundBuff(crewLoadBuff);
        Debug.Log("Artillery: Crew loads 20% faster for rest of round");
        myData.CurrentMana = 0f;
    }

    /// <summary>
    /// Basic Attack: Fire an explosive (AOE) projectile at 1 tile to the right of the nearest enemy.
    /// The offset position is calculated in ScanTargeting and stored in offsetTargetPosition.
    /// This method uses that offset to spawn the projectile, creating detonation 1 tile behind the target.
    /// </summary>
    protected override void PerformBasicAttack()
    {
        if (currentTarget == null) return;

        GameObject prefab = LoadProjectilePrefab();
        if (prefab == null) return;

        GameObject proj = InstantiateAndSetupProjectile(prefab);
        if (proj == null) return;

        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return;

        // Calculate direction and distance towards the offset position (1 tile to the right)
        Vector2 diff = offsetTargetPosition - transform.position;
        Vector2 direction = diff.normalized;
        float launchAngle = myData.BaseDef.LaunchAngle;

        if (launchAngle > 0)
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);
            projScript.speed = CalculateBallisticSpeed(diff, launchAngle, gravity);
        }

        projScript.Setup(myData.GetModifiedDamage(), direction, launchAngle, transform.position, true, this);
    }

    protected override void ScanTargeting()
    {
        if (myData == null)
        {
            currentTarget = null;
            return;
        }

        // Get the nearest target using existing targeting logic
        List<Transform> nearest = GetNearestTargets(1);
        
        if (nearest.Count > 0)
        {
            currentTarget = nearest[0];
            // Offset the target position 1 tile to the right (behind the target)
            offsetTargetPosition = currentTarget.position + Vector3.right;
        }
        else
        {
            currentTarget = null;
        }
    }

    /// <summary>
    /// Find nearby enemies and return the closest ones.
    /// Scoring system prefers enemies not behind the unit.
    /// This method is duplicated from Trebuchet (could be moved to BaseUnit for reuse).
    /// </summary>
    private List<Transform> GetNearestTargets(int maxTargets)
    {
        List<(Transform target, float score)> candidates = new();
        HashSet<int> seenEnemyIds = new();

        LayerMask mask = m_TargetMask.value == 0
            ? LayerMask.GetMask("Enemies")
            : m_TargetMask;

        Camera cam = Camera.main;
        Vector2 bottomLeft = cam.ViewportToWorldPoint(new Vector2(0, 0));
        Vector2 topRight = cam.ViewportToWorldPoint(new Vector2(1, 1));
        Vector2 center = (bottomLeft + topRight) / 2f;
        Vector2 size = topRight - bottomLeft;

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, mask);

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null) continue;

            int enemyId = enemy.GetInstanceID();
            if (!seenEnemyIds.Add(enemyId)) continue;

            Transform enemyTransform = enemy.transform;
            Vector2 delta = enemyTransform.position - transform.position;
            // Slightly penalize enemies behind the unit to prefer forward targets
            bool isBehind = delta.x < -0.05f;
            float score = delta.sqrMagnitude + (isBehind ? 1000f : 0f);
            candidates.Add((enemyTransform, score));
        }

        // Sort by lowest score (closest, not behind)
        candidates.Sort((a, b) => a.score.CompareTo(b.score));

        List<Transform> result = new List<Transform>();
        int takeCount = Mathf.Min(maxTargets, candidates.Count);

        for (int i = 0; i < takeCount; i++)
        {
            result.Add(candidates[i].target);
        }

        return result;
    }
}