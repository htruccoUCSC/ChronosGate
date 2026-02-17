using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class OrbitalLaser : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private GameObject m_ProjectilePrefab;
    
    private float m_AbilityProjectileScaleMultiplier = 5f;

    protected override void CastAbility()
    {
        Debug.Log("OrbitalLaser uses ability");
        // Change to ability when main is updated
SpawnOrbitalLaserAbilityProjectile(myData.GetModifiedDamage() * 2f);
    }

    protected override void PerformBasicAttack()
    {
        myData.CurrentMana+=5;
    }
    private void SpawnOrbitalLaserAbilityProjectile(float damage)
    {
        if (currentTarget == null || m_ProjectilePrefab == null)
        {
        Debug.Log("Orbital laser no asset or target");
        return;
        }

        

       
    }
}


