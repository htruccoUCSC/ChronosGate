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

        SpawnProjectile(m_ProjectilePrefab, myData.GetModifiedDamage() * 2f, true);
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

}

