using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class SniperUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;

    private List<Transform> nearest;

    protected override void CastAbility()
    {
        //Cast shot with 30% damage increase on target buff
        SpawnSniperProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
    }

    protected override void PerformBasicAttack()
    {   
        SpawnSniperProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
    }

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

            // changed: BaseEnemy instead of TargetDummyTest
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null) continue;

            int enemyId = enemy.GetInstanceID();
            if (!seenEnemyIds.Add(enemyId)) continue;

            Transform enemyTransform = enemy.transform;

            // temp scoring: closer is "better" (since we don't read HP)
            float distSqr = (enemyTransform.position - transform.position).sqrMagnitude;
            float score = -distSqr;

            candidates.Add((enemyTransform, score));
        }

        candidates.Sort((a, b) => b.score.CompareTo(a.score));

        List<Transform> result = new();
        int takeCount = Mathf.Min(maxTargets, candidates.Count);

        for (int i = 0; i < takeCount; i++)
            result.Add(candidates[i].target);

        return result;
    }

    protected override void ScanTargeting()
    {
        nearest = GetHighestHealthTarget(1);

        if (nearest.Count > 0)
            currentTarget = nearest[0];
        else
            currentTarget = null;
    }
    protected void SpawnSniperAbilityProjectile(GameObject prefab, float damage, bool isAOE)
{
    if (currentTarget == null || prefab == null) return;

    GameObject projRoot = InstantiateAndSetupProjectile(prefab);
    if (projRoot == null) return;

    Projectile p = projRoot.GetComponentInChildren<Projectile>();
    if (p == null) return;

    Vector2 dir = (currentTarget.position - transform.position).normalized;

    // sniper tuning
    p.speed = 25f;

    // IMPORTANT: make sure we flip trigger on the same collider that will collide
    Collider2D col = p.GetComponent<Collider2D>();
    if (col == null) col = p.GetComponentInChildren<Collider2D>();
    if (col != null) col.isTrigger = true;

    // optionally ignore lane/row checks for sniper
    p.SetIgnoreRowCheck(true);

    // Require sniper projectile to only collide with its assigned target.
    p.SetDesignatedTarget(currentTarget);

    // Let Projectile.Setup set RB type + velocity
    p.Setup(damage, dir, 0f, transform.position, isAOE, this);
    p.EnableOnHitAmp(0.30f);
    }
}
