using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class BountyHunter : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private GameObject m_ProjectilePrefab;

    [SerializeField] private GameObject m_AbilityPrefab;
    private float m_AbilityProjectileScaleMultiplier = 5f;
    private List<Transform> nearest;

    protected override void CastAbility()
    {
        SpawnBountyHunterAbilityProjectile(myData.GetModifiedAbilityPower());
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

            // changed: we target BaseEnemy instead of TargetDummyTest
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null) continue;

            int enemyId = enemy.GetInstanceID();
            if (!seenEnemyIds.Add(enemyId)) continue;

            Transform enemyTransform = enemy.transform;

            // temp scoring: closer is "better" since BaseEnemy doesn't expose HP publicly
            float distSqr = (enemyTransform.position - transform.position).sqrMagnitude;

            // we sort DESC later, so invert distance so closest ends up on top
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

    private void SpawnBountyHunterAbilityProjectile(float damage)
    {
        if (currentTarget == null) return;

        GameObject prefab = LoadProjectilePrefab();
        if (prefab == null)
        {
            Debug.LogError("LoadProjectilePrefab() null");
            return;
        }

        GameObject proj = InstantiateAndSetupProjectile(prefab);
        if (proj == null) return;

        Projectile p = proj.GetComponentInChildren<Projectile>();
        if (p == null)
        {
            Debug.LogError("No Projectile component on spawned proj");
            return;
        }

        // Make it target anything
        p.SetIgnoreRowCheck(true);
        p.SetDesignatedTarget(currentTarget);

        Collider2D col = p.GetComponent<Collider2D>();
        if (col == null) col = p.GetComponentInChildren<Collider2D>();
        if (col != null) col.isTrigger = true;

        // Ability visuals
        var sr = proj.GetComponentInChildren<SpriteRenderer>();
        var animator = proj.GetComponentInChildren<Animator>();
        if (sr != null && animator == null)
            sr.sprite = _abilitySprite != null ? _abilitySprite : _projectileSprite;

        Vector3 scaleToUse = (_abilitySprite != null) ? _abilityScale : _projectileScale;
        proj.transform.localScale =
            Vector3.Scale(transform.localScale, scaleToUse) * m_AbilityProjectileScaleMultiplier;

        // Throw arc
        Vector2 diff = currentTarget.position - transform.position;
        Vector2 direction = diff.normalized;

        float launchAngle = 60f; // steeper arc for catapult
        float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);

        p.speed = CalculateBallisticSpeed(diff, launchAngle, gravity);

        // IMPORTANT: pass this
        p.Setup(damage, direction, launchAngle, transform.position, true, this);

        p.EnableOnHitSlow(1f, 3f);
    }
}