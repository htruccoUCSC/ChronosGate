using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class Tank : BaseUnit
{
    [Header("Ability Settings")]
    [SerializeField] private int m_ShellsPerAbility = 3;
    [SerializeField] private float m_ShellDelay = 0.2f;
    
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;

    protected override void CastAbility()
    {
        Debug.Log("Tank fires 3 shells!");
        StartCoroutine(FireShellSequence(myData.GetModifiedDamage()));
    }

    protected override void PerformBasicAttack()
    {
        // Rarely fires (basic attack minimal, relying on ability)
        if (Random.value < 0.15f) // 15% chance to fire basic shell
        {
            SpawnTankShell(myData.GetModifiedDamage());
        }
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

    private System.Collections.IEnumerator FireShellSequence(float baseDamage)
    {
        for (int i = 0; i < m_ShellsPerAbility; i++)
        {
            SpawnTankShell(baseDamage);
            
            if (i < m_ShellsPerAbility - 1)
            {
                yield return new WaitForSeconds(m_ShellDelay);
            }
        }
    }

    private void SpawnTankShell(float damage)
    {
        if (currentTarget == null) return;

        GameObject projRoot = InstantiateAndSetupProjectile(LoadProjectilePrefab());
        if (projRoot == null) return;

        Projectile p = projRoot.GetComponentInChildren<Projectile>();
        if (p == null) return;

        Vector2 dir = (currentTarget.position - transform.position).normalized;

        p.speed = 15f;

        Collider2D col = p.GetComponent<Collider2D>();
        if (col == null) col = p.GetComponentInChildren<Collider2D>();
        if (col != null) col.isTrigger = true;

        // Ignore row checks so projectiles go directly to target
        p.SetIgnoreRowCheck(true);

        // Tank shell is AOE with straight trajectory
        p.Setup(damage, dir, 0f, transform.position, true, this);
    }
}