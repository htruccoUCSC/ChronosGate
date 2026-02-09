using UnityEngine;

public class ArcherUnit : BaseUnit
{
    // We DO NOT override PerformBasicAttack.
    // The BaseUnit will see "AttackType.Projectile" in the data, 
    // grab the arrow sprite from the "ProjectileTemplate" child, 
    // and fire it automatically.

    // debug to show that abilities are working they just don't do anything yet
    protected override void CastAbility()
    {
        Debug.Log("Archer uses ability");
    }
}
