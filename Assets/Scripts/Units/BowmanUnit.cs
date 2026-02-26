using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

/// <summary>
/// Bowman (HeadHunter) - Targets the highest health (closest) enemy and stuns them.
/// Basic Attack: Shoot using pre-existing sniper projectile method
/// Ability: Same projectile but with stun debuff applied on hit
/// </summary>
public class BowmanUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private float m_StunDuration = 3f;

    private List<Transform> nearest;

    /// <summary>
    /// Ability: Fire a projectile that applies 100% slow (complete stun) for 3 seconds.
    /// Spawns a sniper-style projectile with designated target and applies EnableOnHitSlow(1.0f, 3f).
    /// 100% slow completely immobilizes the target.
    /// </summary>
    protected override void CastAbility()
    {
        if (currentTarget == null) return;

        GameObject prefab = LoadProjectilePrefab();
        if (prefab == null) return;

        GameObject projRoot = InstantiateAndSetupProjectile(prefab);
        if (projRoot == null) return;

        Projectile p = projRoot.GetComponentInChildren<Projectile>();
        if (p == null) return;

        Vector2 dir = (currentTarget.position - transform.position).normalized;
        p.speed = 25f;

        Collider2D col = p.GetComponent<Collider2D>();
        if (col == null) col = p.GetComponentInChildren<Collider2D>();
        if (col != null) col.isTrigger = true;

        p.SetIgnoreRowCheck(true);
        p.SetDesignatedTarget(currentTarget);

        p.Setup(myData.GetModifiedDamage(), dir, 0f, transform.position, false, this);
        p.EnableOnHitSlow(1f, m_StunDuration); // 100% slow = complete stun for 3 seconds

        myData.CurrentMana = 0f;
    }

    /// <summary>
    /// Basic Attack: Fire a standard projectile at the highest health target.
    /// Uses SpawnSniperProjectile() which is designed for single-target precision shots.
    /// </summary>
    protected override void PerformBasicAttack()
    {
        SpawnSniperProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
    }

    /// <summary>
    /// Targets the highest health (closest distance) enemy in view.
    /// This is reused from Sniper/BountyHunter since HP is not exposed on BaseEnemy.
    /// Proximity is used as a proxy for health priority.
    /// </summary>
    public List<Transform> GetHighestHealthTarget(int maxTargets)
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

            // Score by distance: negative distance so closest (smallest) scores highest
            float distSqr = (enemyTransform.position - transform.position).sqrMagnitude;
            float score = -distSqr;

            candidates.Add((enemyTransform, score));
        }

        // Sort descending by score (highest = closest)
        candidates.Sort((a, b) => b.score.CompareTo(a.score));

        List<Transform> result = new();
        int takeCount = Mathf.Min(maxTargets, candidates.Count);

        for (int i = 0; i < takeCount; i++)
            result.Add(candidates[i].target);

        return result;
    }

    /// <summary>
    /// Scan for targets using highest health targeting logic.
    /// Called every frame by BaseUnit to update currentTarget.
    /// </summary>
    protected override void ScanTargeting()
    {
        nearest = GetHighestHealthTarget(1);

        if (nearest.Count > 0)
            currentTarget = nearest[0];
        else
            currentTarget = null;
    }
}