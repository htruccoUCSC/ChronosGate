using System.Collections;
using UnityEngine;

public class Spores : BaseUnit
{
    [Header("Pop row attack")]
    [SerializeField] private int tilesForward = 4;
    [SerializeField] private float tileStep = 1f;        // world units per tile
    [SerializeField] private float popDelay = 0.08f;     // delay between each tile popping (you can change this in inspector as well)
    [SerializeField] private float popLifetime = 0.35f;  // how long each pop stays

    // IMPORTANT: prevent the default BaseUnit arc/basic projectile from firing
    protected override void PerformBasicAttack()
    {
        // Spores basic = nothing (all damage comes from ability pop attack)
    }

    protected override void CastAbility()
    {
        if (myData == null || myData.BaseDef == null) return;

        float dmg = myData.GetModifiedAbilityPower();
        StartCoroutine(PopTilesForward(dmg));
    }

    private IEnumerator PopTilesForward(float dmg)
    {
        GameObject projPrefab = LoadProjectilePrefab();
        if (projPrefab == null) yield break;

        // pop 1..tilesForward in front
        for (int i = 1; i <= tilesForward; i++)
        {
            Vector3 pos = transform.position + Vector3.right * (tileStep * i);

            // Spawn the projectile prefab at the tile position
            GameObject projRoot = Instantiate(projPrefab, pos, Quaternion.identity);

            // Find the Projectile component (root or child)
            Projectile p = projRoot.GetComponentInChildren<Projectile>();
            if (p != null)
            {
                p.speed = 0f;
                p.lifetime = popLifetime;

                // Ensure trigger collisions happen
                Collider2D col = p.GetComponent<Collider2D>();
                if (col == null) col = p.GetComponentInChildren<Collider2D>();
                if (col != null) col.isTrigger = true;

                p.SetIgnoreRowCheck(true);

                p.Setup(dmg, Vector2.right, 0f, pos, true, this);
            }

            // Cleanup in case lifetime isn’t respected for some reason
            Destroy(projRoot, popLifetime + 0.05f);

            if (i < tilesForward)
                yield return new WaitForSeconds(popDelay);
        }
    }
}