using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(MeleeAttackBehavior))]
public class Mech : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private GameObject m_ProjectilePrefab;

    [Header("Orbital Settings")]
    [SerializeField] private GameObject m_OrbitalPrefab;
    [SerializeField] private float m_OrbitalLifetime = 5f;
    // divisor used in a soft-saturating mapping: percent = abilityPower / (abilityPower + divisor)
    // set so that abilityPower==50 -> percent ~ 0.15
    [SerializeField] private float m_AbilityToPercentDivisor = 283.3333333f;
    [Tooltip("Maximum percent of unoccupied tiles that can be spawned on (0..1)")]
    [Range(0f,1f)]
    [SerializeField] private float m_MaxSpawnPercent = 0.5f;

    protected override void CastAbility()
    {
        Debug.Log("Mech uses ability");
        
        float abilityPower = myData.GetModifiedAbilityPower();
        // soft-saturating mapping so that percent = abilityPower / (abilityPower + divisor)
        float percentAP = 0f;
        if (m_AbilityToPercentDivisor > 0f)
            percentAP = abilityPower / (abilityPower + m_AbilityToPercentDivisor);

        Debug.Log($"Mech.CastAbility: abilityPower={abilityPower}, divisor={m_AbilityToPercentDivisor}, percentAP={percentAP}");
        
        GameObject prefabToSpawn = (m_OrbitalPrefab != null) ? m_OrbitalPrefab : m_ProjectilePrefab;

        // If the chosen prefab doesn't contain a Projectile component, fall back to the default projectile prefab.
        if (prefabToSpawn != null)
        {
            var hasProj = prefabToSpawn.GetComponentInChildren<Projectile>() != null;
            if (!hasProj)
            {
                if (m_ProjectilePrefab != null && m_ProjectilePrefab != prefabToSpawn)
                {
                    Debug.Log($"Mech.CastAbility: chosen prefab '{prefabToSpawn.name}' has no Projectile; falling back to '{m_ProjectilePrefab.name}'.");
                    prefabToSpawn = m_ProjectilePrefab;
                }
                else
                {
                    Debug.LogWarning($"Mech.CastAbility: chosen prefab '{prefabToSpawn.name}' has no Projectile and no fallback projectile prefab is assigned.");
                }
            }
        }

        if (prefabToSpawn != null && percentAP > 0f)
        {
            float capped = Mathf.Min(percentAP, m_MaxSpawnPercent);
            TileSpawner.SpawnPercentUnoccupiedTiles(capped, prefabToSpawn, m_OrbitalLifetime, m_TargetMask);
            Debug.Log($"Mech ability requested spawn on {capped * 100f}% (raw {percentAP * 100f}%) of unoccupied tiles using prefab: {prefabToSpawn.name}");
        }

        myData.CurrentMana = 0f;
    }

    protected override void PerformBasicAttack()
    {
        if(!TryPerformMeleeAttack(myData.GetModifiedDamage(), 2f))
        {
            SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), false);

            // Ensure mech projectiles pass through enemies and are cleaned up when off-screen
            if (spawnedProjectiles != null && spawnedProjectiles.Count > 0)
            {
                GameObject last = spawnedProjectiles[spawnedProjectiles.Count - 1];
                if (last != null)
                {
                    Projectile p = last.GetComponentInChildren<Projectile>();
                    if (p != null)
                    {
                        p.passThroughEnemies = true;
                    }
                }
            }
        }
        
    }
    
}

