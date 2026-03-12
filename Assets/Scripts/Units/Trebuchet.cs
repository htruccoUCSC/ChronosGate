using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class TrebuchetUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private float m_AbilityBurstDelay = 0.2f;

    protected override void CastAbility()
    {
        Debug.Log("Trebuchet uses ability");

        if (LoadProjectilePrefab() == null) return;

        StartCoroutine(FireAbilityBursts(myData.GetModifiedDamage(), 3, 2));
        myData.CurrentMana = 0f;
    }

    protected override void PerformBasicAttack()
    {
        Debug.Log("myData.getModifiedAttackSpeed(): " + myData.GetModifiedAttackSpeed());
            Debug.Log($"[Trebuchet] Before attack: getModifiedAttackSpeed()={myData.GetModifiedAttackSpeed()}, SpeedFlatMod={myData.SpeedFlatMod}, SpeedMultMod={myData.SpeedMultMod}");
        if (LoadProjectilePrefab() == null) return;

        float damage = myData.GetModifiedDamage();
        List<Transform> targets = GetNearestTargets(2);

        foreach (Transform target in targets)
        {
            SpawnProjectileAtTarget(target, damage, false);
        }
            Debug.Log($"[Trebuchet] After attack: getModifiedAttackSpeed()={myData.GetModifiedAttackSpeed()}, SpeedFlatMod={myData.SpeedFlatMod}, SpeedMultMod={myData.SpeedMultMod}");
    }

    private IEnumerator FireAbilityBursts(float damage, int burstCount, int targetsPerBurst)
    {
        if (LoadProjectilePrefab() == null) yield break;

        for (int burstIndex = 0; burstIndex < burstCount; burstIndex++)
        {
            List<Transform> targets = GetNearestTargets(targetsPerBurst);
            foreach (Transform target in targets)
            {
                SpawnProjectileAtTarget(target, damage, false);
            }

            if (burstIndex < burstCount - 1)
            {
                yield return new WaitForSeconds(m_AbilityBurstDelay);
            }
        }
    }

    private void SpawnProjectileAtTarget(Transform target, float damage, bool isAoe)
    {
        if (target == null) return;

        GameObject projectilePrefab = LoadProjectilePrefab();
        if (projectilePrefab == null) return;

        GameObject proj = InstantiateAndSetupProjectile(projectilePrefab);
        if (proj == null) return;

        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return;

        Vector2 diff = target.position - transform.position;
        Vector2 direction = diff.normalized;
        float launchAngle = myData.BaseDef.LaunchAngle;

        if (launchAngle > 0)
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);
            projScript.speed = CalculateBallisticSpeed(diff, launchAngle, gravity);
        }

        float angleOffset = UnityEngine.Random.Range(-5f, 5f);
        launchAngle += angleOffset;

        projScript.Setup(damage, direction, launchAngle, transform.position, isAoe, this);
        projScript.SetIgnoreRowCheck(true);
        projScript.SetDesignatedTarget(target);
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
            if (hit == null) continue;

            // Try BaseEnemy first, fall back to TargetDummyTest for legacy/enemy prefabs
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            TargetDummyTest dummy = null;
            if (enemy == null)
            {
                dummy = hit.GetComponentInParent<TargetDummyTest>();
                if (dummy == null) continue;
            }

            int enemyId = (enemy != null) ? enemy.GetInstanceID() : dummy.GetInstanceID();
            if (!seenEnemyIds.Add(enemyId)) continue;

            Transform enemyTransform = (enemy != null) ? enemy.transform : dummy.transform;
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
}
