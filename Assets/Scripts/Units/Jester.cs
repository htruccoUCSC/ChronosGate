using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class Jester : BaseUnit
{
    [Header("Ability Settings")]
    [SerializeField] private float m_BuffDuration = 8f;
    [SerializeField] private int m_BuffDurationMin = 3;
    [SerializeField] private int m_BuffDurationMax = 6;
    
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;

    private Buffs m_BuffSystem;

    private void Awake()
    {
        m_BuffSystem = FindFirstObjectByType<Buffs>();
    }

    protected override void CastAbility()
    {
        Debug.Log("Jester uses mystical ability!");
        
        // Find all adjacent allies
        List<BaseUnit> adjacentAllies = GetAdjacentAllies();

        if (adjacentAllies.Count == 0)
        {
            Debug.Log("Jester: No adjacent allies found!");
            return;
        }

        if (m_BuffSystem == null)
        {
            m_BuffSystem = FindFirstObjectByType<Buffs>();
        }

        if (m_BuffSystem == null)
        {
            Debug.LogWarning("Jester ability skipped: Buffs system not found.");
            return;
        }

        // Pick one random adjacent ally
        BaseUnit luckyAlly = adjacentAllies[UnityEngine.Random.Range(0, adjacentAllies.Count)];

        // Random multiplier 1x to 6x for ALL stats
        float statMultiplier = UnityEngine.Random.Range(1f, 7f);
        int duration = UnityEngine.Random.Range(m_BuffDurationMin, m_BuffDurationMax + 1);

        Debug.Log($"Jester blessed {luckyAlly.name} with {statMultiplier:F1}x stats for {duration} seconds!");

        // Apply buff with proper Action<float> parameters
        m_BuffSystem.AddTempBuff(
            luckyAlly,
            statMultiplier * 0.25f,  // attack speed multiplier
            statMultiplier * 5f,      // attack speed flat
            statMultiplier * 5f,      // attack damage flat
            statMultiplier * 0.2f,    // attack damage multiplier
            statMultiplier * 10f,     // ability power flat
            statMultiplier * 0.3f,    // ability power multiplier
            0f,                       // range buff (no scaling)
            duration,
            null,                     // OnHit action (no action)
            0f,                       // onHitModifier
            null,                     // OnKill action (no action)
            0f,                       // onKillModifier
            false,                    // calledFromAugment
            false                     // refreshOnPlacement
        );
    }

    protected override void PerformBasicAttack()
    {
        // High single target damage
        if (currentTarget == null) return;

        SpawnJesterProjectile(myData.GetModifiedDamage() * 1.5f);
    }

    protected override void ScanTargeting()
    {
        // Target any enemy location on the map (borrowed from Sniper)
        List<Transform> targets = GetAllEnemyTargets(1);
        
        if (targets.Count > 0)
            currentTarget = targets[0];
        else
            currentTarget = null;
    }

    private List<Transform> GetAllEnemyTargets(int maxTargets)
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

    private List<BaseUnit> GetAdjacentAllies()
    {
        List<BaseUnit> adjacent = new();
        float adjacencyRange = 1.5f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, adjacencyRange, LayerMask.GetMask("Units"));

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            BaseUnit unit = hit.GetComponent<BaseUnit>();
            if (unit == null || unit == this) continue;

            adjacent.Add(unit);
        }

        return adjacent;
    }

    private void SpawnJesterProjectile(float damage)
    {
        if (currentTarget == null) return;

        GameObject projRoot = InstantiateAndSetupProjectile(LoadProjectilePrefab());
        if (projRoot == null) return;

        Projectile p = projRoot.GetComponentInChildren<Projectile>();
        if (p == null) return;

        Vector2 dir = (currentTarget.position - transform.position).normalized;

        p.speed = 18f;

        Collider2D col = p.GetComponent<Collider2D>();
        if (col == null) col = p.GetComponentInChildren<Collider2D>();
        if (col != null) col.isTrigger = true;

        // Ignore row checks so projectiles go directly to target
        p.SetIgnoreRowCheck(true);

        // Straight shot (no arc)
        p.Setup(damage, dir, 0f, transform.position, false, this);
    }
}