using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class OrbitalLaser : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private GameObject m_ProjectilePrefab;
    
    private float m_AbilityProjectileScaleMultiplier = 5f;

    protected override void CastAbility()
    {
        Debug.Log("OrbitalLaser uses ability");
        // Change to ability when main is updated
SpawnOrbitalLaserAbilityProjectile(myData.GetModifiedDamage() * 2f);
    }

    protected override void PerformBasicAttack()
    {
        myData.CurrentMana+=5;
    }
    private void SpawnOrbitalLaserAbilityProjectile(float damage)
    {
        if (currentTarget == null || m_ProjectilePrefab == null)
        {
        Debug.Log("Orbital laser no asset or target");
        return;
        }

        List<Transform> target = GetNearestTargets(1);
        SpawnProjectileAtTarget(target[0],5,true);
    }
    private List<Transform> GetNearestTargets(int maxTargets)
    {
        List<(Transform target, float score)> candidates = new List<(Transform target, float score)>();
        HashSet<int> seenEnemyIds = new HashSet<int>();

        LayerMask mask = m_TargetMask.value == 0 ? LayerMask.GetMask("Enemies") : m_TargetMask;
        float range = myData.BaseDef.Range;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, mask);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.CompareTag("Enemy")) continue;

            TargetDummyTest enemy = hit.GetComponentInParent<TargetDummyTest>();
            if (enemy == null) continue;

            int enemyId = enemy.GetInstanceID();
            if (!seenEnemyIds.Add(enemyId)) continue;

            Transform enemyTransform = enemy.transform;
            Vector2 delta = enemyTransform.position - transform.position;
            bool isBehind = delta.x < -0.05f;
            float score = delta.sqrMagnitude + (isBehind ? 1000f : 0f);
            candidates.Add((enemyTransform, score));
        }

        candidates.Sort((a, b) => a.score.CompareTo(b.score));

        List<Transform> result = new List<Transform>();
        int takeCount = Mathf.Min(maxTargets, candidates.Count);

        for (int i = 0; i < takeCount; i++)
        {
            result.Add(candidates[i].target);
        }

        return result;
    }
    private void SpawnProjectileAtTarget(Transform target, float damage, bool isAoe)
    {
        if (target == null || m_ProjectilePrefab == null) return;

        GameObject proj = Instantiate(m_ProjectilePrefab, transform.position, Quaternion.identity);
        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        Vector2 diff = new Vector2(0f, 0f);
        projScript.Setup(damage, diff, 15, transform.position, isAoe);
        projScript.SetIgnoreRowCheck(true);
        projScript.SetDesignatedTarget(target);
        OrbitalLaserBehavior laserBehavior = proj.GetComponent<OrbitalLaserBehavior>();
        laserBehavior.Initialize(projScript, currentTarget, 5,3,2,2,m_TargetMask);
    }
    
}


