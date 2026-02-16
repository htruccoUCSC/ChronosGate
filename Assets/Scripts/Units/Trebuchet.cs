using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;

public class TrebuchetUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private float m_AbilityBurstDelay = 0.2f;


    protected override void CastAbility()
    {
        Debug.Log("Trebuchet uses ability");
        if (m_ProjectilePrefab == null)
        {
            Debug.LogError("Trebuchet projectile prefab is not assigned.");
            return;
        }

        StartCoroutine(FireAbilityBursts(myData.GetModifiedDamage(), 3, 2));
    }

    protected override void PerformBasicAttack()
    {
        if (m_ProjectilePrefab == null)
        {
            Debug.LogError("Trebuchet projectile prefab is not assigned.");
            return;
        }

        float damage = myData.GetModifiedDamage();
        List<Transform> targets = GetNearestTargets(2);

        foreach (Transform target in targets)
        {
            SpawnProjectileAtTarget(target, damage, false);
        }
    }

    private IEnumerator FireAbilityBursts(float damage, int burstCount, int targetsPerBurst)
    {
        if (m_ProjectilePrefab == null) yield break;

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
        if (target == null || m_ProjectilePrefab == null) return;

        GameObject proj = Instantiate(m_ProjectilePrefab, transform.position, Quaternion.identity);

        if (_projectileSprite != null)
        {
            var sr = proj.GetComponentInChildren<SpriteRenderer>();
            var animator = proj.GetComponentInChildren<Animator>();
            if (sr != null && animator == null) sr.sprite = _projectileSprite;
            proj.transform.localScale = Vector3.Scale(transform.localScale, _projectileScale);
        }

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
        //offset angle modifier
        float angleOffset = Random.Range(-5f, 5f);
        launchAngle += angleOffset;
        projScript.Setup(damage, direction, launchAngle, transform.position, isAoe);
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

