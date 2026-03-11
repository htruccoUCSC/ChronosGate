using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Spores : BaseUnit
{
    [Header("Pop row attack")]
    [SerializeField] private int tilesForward = 4;
    [SerializeField] private float tileStep = 1f;
    [SerializeField] private float popDelay = 0.08f;
    [SerializeField] private float popLifetime = 0.35f;

    protected override void ScanTargeting()
    {
        currentTarget = transform;
    }

    protected override void PerformBasicAttack()
    {
        // Spores basic = nothing; all damage comes from its pop ability.
    }

    protected override void CastAbility()
    {
        if (myData == null || myData.BaseDef == null) return;

        float damage = myData.GetModifiedAbilityPower();
        StartCoroutine(PopTilesForward(damage));
    }

    private IEnumerator PopTilesForward(float damage)
    {
        GameObject projectilePrefab = LoadProjectilePrefab();
        if (projectilePrefab == null) yield break;

        for (int i = 1; i <= tilesForward; i++)
        {
            Vector3 worldPosition = transform.position + Vector3.right * (tileStep * i);
            GameObject projectileRoot = Instantiate(projectilePrefab, worldPosition, Quaternion.identity);

            Projectile projectile = projectileRoot.GetComponentInChildren<Projectile>();
            if (projectile != null)
            {
                projectile.speed = 0f;
                projectile.lifetime = popLifetime;

                Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
                if (projectileCollider == null)
                {
                    projectileCollider = projectile.GetComponentInChildren<Collider2D>();
                }

                if (projectileCollider != null)
                {
                    projectileCollider.isTrigger = true;
                }

                projectile.SetIgnoreRowCheck(true);
                projectile.Setup(damage, Vector2.right, 0f, worldPosition, true, this);
            }

            // Stationary pop hitboxes can spawn already overlapping enemies, so deal damage immediately.
            ApplyPopDamageAtPosition(worldPosition, damage);

            Destroy(projectileRoot, popLifetime + 0.05f);

            if (i < tilesForward)
            {
                yield return new WaitForSeconds(popDelay);
            }
        }
    }

    private void ApplyPopDamageAtPosition(Vector3 worldPosition, float damage)
    {
        int dealt = Mathf.Max(1, Mathf.RoundToInt(damage));
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, 0.45f, LayerMask.GetMask("Enemies"));
        HashSet<int> seenTargets = new HashSet<int>();
        Tilemap tilemap = WaveManager.Instance != null ? WaveManager.Instance.tilemap : null;
        int popRow = tilemap != null ? tilemap.WorldToCell(worldPosition).y : 0;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null) continue;

            BaseEnemy enemy = hit.GetComponentInParent<BaseEnemy>();
            if (enemy != null)
            {
                if (tilemap != null && tilemap.WorldToCell(enemy.transform.position).y != popRow) continue;
                if (!seenTargets.Add(enemy.GetInstanceID())) continue;

                OnHit();
                int finalDamage = Mathf.RoundToInt(dealt * enemy.DamageAmp);
                enemy.TakeDamage(this, finalDamage);
                continue;
            }

            TargetDummyTest dummy = hit.GetComponentInParent<TargetDummyTest>();
            if (dummy == null) continue;
            if (tilemap != null && tilemap.WorldToCell(dummy.transform.position).y != popRow) continue;
            if (!seenTargets.Add(dummy.GetInstanceID())) continue;

            OnHit();
            dummy.TakeDamage(dealt, this);
        }
    }
}
