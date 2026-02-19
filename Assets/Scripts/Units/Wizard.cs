using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class WizardUnit : BaseUnit
{
    [SerializeField] private LayerMask m_TargetMask;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private Sprite m_SheepSprite;
    [SerializeField] private float m_PolymorphDuration = 2f;
    [SerializeField] private float m_PolymorphSlowPercent = 0.5f;

    protected override void CastAbility()
    {
        Debug.Log("Wizard uses ability");

        if (currentTarget == null)
        {
            return;
        }

        TargetDummyTest targetDummy = currentTarget.GetComponentInParent<TargetDummyTest>();
        if (targetDummy == null)
        {
            return;
        }

        if (m_SheepSprite == null)
        {
            Debug.LogWarning("Wizard sheep sprite is not assigned.");
            return;
        }

        targetDummy.ApplyPolymorph(m_SheepSprite, m_PolymorphDuration);
        targetDummy.ApplySlow(m_PolymorphSlowPercent, m_PolymorphDuration);
    }

    protected override void PerformBasicAttack()
    {
        SpawnProjectile(LoadProjectilePrefab(), myData.GetModifiedDamage(), true);
    }

    
}

