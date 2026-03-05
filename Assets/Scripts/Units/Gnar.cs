using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boomerang : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private float m_SpreadAngle = 12f;
    [SerializeField] private int m_ProjectilesPerAttack = 1;

    private BoardManager m_Board;

    protected override void CastAbility()
    {
        Debug.Log("Boomerang uses ability");
        SpawnArcApexProjectile(myData.GetModifiedDamage() , true);
    }

    protected override void PerformBasicAttack()
    {
        float damage = myData.GetModifiedDamage();
        SpawnSpreadAttack(damage);
    }

    protected override void ScanTargeting()
    {
        if (myData == null)
        {
            currentTarget = null;
            return;
        }

        LayerMask mask = m_TargetMask.value == 0 ? LayerMask.GetMask("Enemies") : m_TargetMask;
        float range = myData.BaseDef.Range;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, mask);

        Transform bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;

            // changed: dont rely on tag, just make sure its actually an enemy
            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy == null) continue;

            Vector2 toEnemy = enemy.transform.position - transform.position;
            // Ignore enemies behind this unit.
            if (toEnemy.x <= 0.05f) continue;
            float score = toEnemy.sqrMagnitude;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = enemy.transform;
            }
        }

        currentTarget = bestTarget;
    }

    private void Awake()
    {
        m_Board = FindFirstObjectByType<BoardManager>();
    }

    private void SpawnSpreadAttack(float damage)
    {
        if (currentTarget == null) return;

        int projectileCount = Mathf.Clamp(m_ProjectilesPerAttack, 1, 3);
        if (projectileCount == 1)
        {
            SpawnArcApexProjectile(damage, false, 0f);
            return;
        }

        if (projectileCount == 2)
        {
            SpawnArcApexProjectile(damage, false, -m_SpreadAngle * 0.5f);
            SpawnArcApexProjectile(damage, false, m_SpreadAngle * 0.5f);
            return;
        }

        SpawnArcApexProjectile(damage, false, -m_SpreadAngle);
        SpawnArcApexProjectile(damage, false, 0f);
        SpawnArcApexProjectile(damage, false, m_SpreadAngle);
    }

    private void SpawnArcApexProjectile(float damage, bool isAoe, float angleOffset = 0f)
    {
        GameObject projectilePrefab = LoadProjectilePrefab();
        if (currentTarget == null || projectilePrefab == null) return;

        GameObject proj = InstantiateAndSetupProjectile(projectilePrefab);
        if (proj == null) return;

        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return;

        Vector2 diff = currentTarget.position - transform.position;
        float distance = diff.magnitude;
        Vector2 direction = diff.normalized;
        float launchAngle = myData.BaseDef.LaunchAngle + angleOffset;

        if (launchAngle > 0)
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);
            projScript.speed = CalculateBallisticSpeed(distance, launchAngle, gravity);
        }

        projScript.Setup(damage, direction, launchAngle, transform.position, isAoe, this);

        LayerMask mask = m_TargetMask.value == 0 ? LayerMask.GetMask("Enemies") : m_TargetMask;
        projScript.EnableApexRetarget(mask, myData.BaseDef.Range * 4f, true);

        BoomerangProjectileBehavior boomerBehavior = proj.GetComponent<BoomerangProjectileBehavior>();
        if (boomerBehavior == null)
        {
            boomerBehavior = proj.AddComponent<BoomerangProjectileBehavior>();
        }

        boomerBehavior.Initialize(projScript, currentTarget, transform);
    }
}
