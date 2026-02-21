using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Boomerang : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private float m_SpreadAngle = 12f;
    [SerializeField] private float m_MoveDelay = 0.5f;

    private BoardManager m_Board;
    private bool m_MoveScheduled;


    protected override void CastAbility()
    {
        Debug.Log("Boomerang uses ability");
        SpawnArcApexProjectile(myData.GetModifiedDamage() * 2f, true);
    }

    protected override void PerformBasicAttack()
    {
        float damage = myData.GetModifiedDamage();
        SpawnSpreadAttack(damage);

        if (!m_MoveScheduled)
        {
            StartCoroutine(MoveToAdjacentTileAfterDelay(m_MoveDelay));
        }

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
            if (hit == null || !hit.CompareTag("Enemy")) continue;

            Vector2 toEnemy = hit.transform.position - transform.position;

            // Prefer enemies in front of the unit, but still allow cross-lane targeting.
            bool isBehind = toEnemy.x < -0.05f;
            float score = toEnemy.sqrMagnitude + (isBehind ? 1000f : 0f);

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = hit.transform;
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

        projScript.Setup(damage, direction, launchAngle, transform.position, isAoe);

        LayerMask mask = m_TargetMask.value == 0 ? LayerMask.GetMask("Enemies") : m_TargetMask;
        projScript.EnableApexRetarget(mask, myData.BaseDef.Range * 4f, true);

        BoomerangProjectileBehavior boomerBehavior = proj.GetComponent<BoomerangProjectileBehavior>();
        if (boomerBehavior == null)
        {
            boomerBehavior = proj.AddComponent<BoomerangProjectileBehavior>();
        }

        boomerBehavior.Initialize(projScript, currentTarget, transform);
    }

    private IEnumerator MoveToAdjacentTileAfterDelay(float delay)
    {
        m_MoveScheduled = true;
        yield return new WaitForSeconds(delay);

        if (m_Board == null || m_Board.GameTilemap == null)
        {
            m_MoveScheduled = false;
            yield break;
        }

        if (!m_Board.TryGetUnitCell(gameObject, out Vector3Int currentCell))
        {
            m_MoveScheduled = false;
            yield break;
        }

        Vector3Int[] offsets =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        List<Vector3Int> availableCells = new List<Vector3Int>();
        foreach (Vector3Int offset in offsets)
        {
            Vector3Int candidate = currentCell + offset;
            if (m_Board.IsWalkable(candidate))
            {
                availableCells.Add(candidate);
            }
        }

        if (availableCells.Count > 0)
        {
            Vector3Int nextCell = availableCells[Random.Range(0, availableCells.Count)];
            m_Board.MoveUnit(gameObject, nextCell);
        }

        m_MoveScheduled = false;
    }

}

