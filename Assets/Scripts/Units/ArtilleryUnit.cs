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

    protected override void CastAbility()
    {
        // Create a round-long buff that increases attack speed by 20%.
        // This makes the unit fire 20% faster (shorter cooldown between attacks).
        Buff crewLoadBuff = new Buff
        {
            AttackSpeedFlat = myData.GetModifiedAttackSpeed() * 0.20f,
            duration = float.MaxValue // Lasts for entire round
        };
        
        AddRoundBuff(crewLoadBuff);
        Debug.Log("Artillery: Crew loads 20% faster for rest of round");
        myData.CurrentMana = 0f;
    }

    /// <summary>
    /// Basic Attack: Fire an explosive (AOE) projectile at the nearest enemy.
    /// Uses ballistic trajectory for dramatic effect.
    /// </summary>
    protected override void PerformBasicAttack()
    {
        if (currentTarget == null) return;
        SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), true);
    }
/*
    protected override void ScanTargeting()
    {
        if (myData == null)
        {
            currentTarget = null;
            return;
        }

        List<Transform> nearest = GetNearestTargets(1);
        currentTarget = nearest.Count > 0 ? nearest[0] : null;
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
    */
}