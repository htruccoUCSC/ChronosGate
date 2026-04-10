using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boomerang : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private float m_SpreadAngle = 12f;
    [Header("Ability")]
    [SerializeField] private float m_AbilityBoomerangScale;
    [SerializeField] private float m_AbilitySlowPercent = 0.3f;
    [SerializeField] private float m_AbilitySlowDuration = 2f;

    private BoardManager m_Board;

    protected override void CastAbility()
    {
        Debug.Log("Boomerang uses ability");
        m_AbilityBoomerangScale = myData.GetModifiedAbilityPower()/75 * 0.5f + 1f; // scale based on ability power, minimum 1x
        Projectile proj = SpawnArcApexProjectile(myData.GetModifiedDamage(), true, 0f, m_AbilityBoomerangScale);
        if (proj != null)
        {
            Debug.Log($"Boomerang ability projectile spawned with scale {m_AbilityBoomerangScale:F2} Slow Enabled");
            proj.EnableOnHitSlow(m_AbilitySlowPercent, m_AbilitySlowDuration);
        }
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


        //75% chance to shoot 1 projectile, 20% chance to shoot 2 projectiles, 5% chance to shoot 3 projectiles
        int projectileCount = 1;
        float rand = Random.value;
        if (rand < 0.75f)
        {
            projectileCount = 1;
        }
        else if (rand < 0.95f)
        {
            projectileCount = 2;
        }
        else
        {
            projectileCount = 3;
        }
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

        if (projectileCount >= 3)
        {
            SpawnArcApexProjectile(damage, false, -m_SpreadAngle);
            SpawnArcApexProjectile(damage, false, 0f);
            SpawnArcApexProjectile(damage, false, m_SpreadAngle);
            return;
        }
    }


    private Projectile SpawnArcApexProjectile(float damage, bool isAoe, float angleOffset = 0f, float scaleMultiplier = 1f)
    {
        GameObject projectilePrefab = LoadProjectilePrefab();
        if (currentTarget == null || projectilePrefab == null) return null;

        GameObject proj = InstantiateAndSetupProjectile(projectilePrefab);
        if (proj == null) return null;

        if (Mathf.Abs(scaleMultiplier - 1f) > 0.001f)
        {
            proj.transform.localScale = proj.transform.localScale * Mathf.Max(0.01f, scaleMultiplier);
        }

        Projectile projScript = proj.GetComponentInChildren<Projectile>();
        if (projScript == null) return null;

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
        return projScript;
    }
}
