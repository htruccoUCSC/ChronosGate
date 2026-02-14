using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class RockmanUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private GameObject m_ProjectilePrefab;


    protected override void CastAbility()
    {
        Debug.Log("Rock-man uses ability");
        if (m_ProjectilePrefab == null)
        {
            Debug.LogError("Rockman projectile prefab is not assigned.");
            return;
        }

        SpawnRockmanAbilityProjectile(myData.GetModifiedDamage() * 2f);
    }

    protected override void PerformBasicAttack()
    {
        if (m_ProjectilePrefab == null)
        {
            Debug.LogError("Rockman projectile prefab is not assigned.");
            return;
        }

        SpawnProjectile(m_ProjectilePrefab, myData.GetModifiedDamage(), false);
    }

    private void SpawnRockmanAbilityProjectile(float damage)
    {
        if (currentTarget == null || m_ProjectilePrefab == null) return;

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

        Vector2 diff = currentTarget.position - transform.position;
        float distance = diff.magnitude;
        Vector2 direction = diff.normalized;
        float launchAngle = myData.BaseDef.LaunchAngle;

        if (launchAngle > 0)
        {
            float gravity = Mathf.Abs(Physics2D.gravity.y * 3f);
            projScript.speed = CalculateBallisticSpeed(distance, launchAngle, gravity);
        }

        projScript.Setup(damage, direction, launchAngle, transform.position, true);
        projScript.EnableOnHitSlow(0.30f, 3f);
    }

}

