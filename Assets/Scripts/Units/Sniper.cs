using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class SniperUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;

private List<Transform> nearest;

    protected override void CastAbility()
    {
        //Cast shot with 30% damage increase on target buff
          SpawnSniperProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
    }

    protected override void PerformBasicAttack()
    {

        SpawnSniperProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);
        //if (nearest[0]!=null){
         //TargetDummyTest enemy = nearest[0].GetComponentInParent<TargetDummyTest>();
         //enemy.TakeDamage(Mathf.RoundToInt( myData.GetModifiedDamage()));
        //}
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
    Vector2 topRight   = cam.ViewportToWorldPoint(new Vector2(1, 1));

    Vector2 center = (bottomLeft + topRight) / 2f;
    Vector2 size   = topRight - bottomLeft;

    Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f, mask);

    foreach (Collider2D hit in hits)
    {
        if (hit == null || !hit.CompareTag("Enemy")) 
            continue;

        TargetDummyTest enemy = hit.GetComponentInParent<TargetDummyTest>();
        if (enemy == null) 
            continue;

        int enemyId = enemy.GetInstanceID();
        if (!seenEnemyIds.Add(enemyId)) 
            continue;

        float score = enemy.maxHealth;
         Transform enemyTransform = enemy.transform;
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
}


