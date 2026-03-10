using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;
using System.Collections;

public class OrbitalLaser : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private GameObject m_ProjectilePrefab;
    OrbitalLaserBehavior orbitalLaserBehavior;
    public float orbitalLaserLifetime = 3;

    protected override void CastAbility()
    {
        List<Transform> nearest = GetNearestTargets(1);

        if (nearest.Count > 0 && nearest[0] != null)
        {
            GameObject obj = InstantiateAndSetupProjectile(getOrbitalLaser());
            OrbitalLaserBehavior behavior = obj.GetComponent<OrbitalLaserBehavior>();

            behavior.Initialize(
                nearest[0],
                myData.GetModifiedAbilityPower()*0.4f,
                orbitalLaserLifetime, //lifetime
                1f,//movespeed
                0.5f, //radius
                m_TargetMask,
                this
            );
        }
        else
        {
            Debug.Log("no enemies found");
        }
    }

    protected override void PerformBasicAttack()
    {
        myData.CurrentMana += 5;
    }

    public GameObject getOrbitalLaser()
    {
        return LoadProjectilePrefab();
    }

    public List<Transform> GetNearestTargets(int maxTargets)
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
            if (hit == null || !hit.CompareTag("Enemy"))
                continue;

            // CHANGED: TargetDummyTest -> BaseEnemy
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null)
                continue;

            int enemyId = enemy.GetInstanceID();
            if (!seenEnemyIds.Add(enemyId))
                continue;

            Transform enemyTransform = enemy.transform;
            Vector2 delta = enemyTransform.position - transform.position;
            float score = delta.sqrMagnitude;

            candidates.Add((enemyTransform, score));
        }

        candidates.Sort((a, b) => a.score.CompareTo(b.score));

        List<Transform> result = new();
        int takeCount = Mathf.Min(maxTargets, candidates.Count);

        for (int i = 0; i < takeCount; i++)
            result.Add(candidates[i].target);

        return result;
    }

    protected override void ScanTargeting()
    {
        List<Transform> nearest = GetNearestTargets(1);

        if (nearest.Count > 0)
            currentTarget = nearest[0];
        else
            currentTarget = null;
    }
}