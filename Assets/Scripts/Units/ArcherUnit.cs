using UnityEngine;

// Inherits from BaseUnit
public class ArcherUnit : BaseUnit
{
    protected override void PerformBasicAttack()
    {
        // Access stats.AttackDamage here
        Debug.Log($"{stats.Name} shoots, Dmg: {stats.AttackDamage}. Mana: {currentMana}/ {stats.MaxMana}");
    }

    protected override void CastAbility()
    {
        // Access stats.AbilityPower here
        Debug.Log($"<color=green>{stats.Name} casts ability, Dmg: {stats.AbilityPower}</color>");
    }
}