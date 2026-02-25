using UnityEngine;

public class UnitInstance : ScriptableObject
{
    // pointer to the base definition (shared data)
    public UnitDefinition BaseDef;
    public string Faction;
    public float MaxHP;
    public float CurrentHP;
    public float CurrentMana;
    public float StartingMana;
    // modifiers from augments and shit
    public float DamageMultMod = 1.0f;
    public float DamageFlatMod = 0;
    public float SpeedFlatMod = 0;
    public float SpeedMultMod = 1.0f;
    public float BaseAbilityPower = 0;
    public float AbilityPowerFlatMod = 0;
    public float AbilityPowerMult=1.0f;


    // this is basically a constructor for creating runtime instances
    // needs to copy more data over and return modified values
    public static UnitInstance CreateRuntimeInstance(UnitDefinition def)
    {
        UnitInstance instance = ScriptableObject.CreateInstance<UnitInstance>();
        instance.BaseDef = def;
        instance.StartingMana = def.StartingMana;
        instance.CurrentMana = def.StartingMana;
        instance.Faction= def.Faction;
        instance.BaseAbilityPower = def.AbilityPower;
        instance.AbilityPowerFlatMod = 0f;
        instance.MaxHP = def.Health; 
        instance.CurrentHP = instance.MaxHP; 
        return instance;
    }

    // equations for modified stats with buffs
    //Clamp to prervent negative or zero values where it would break things
    public float GetModifiedAttackSpeed() => Mathf.Max(0.1f, (BaseDef.AttackSpeed + SpeedFlatMod) * SpeedMultMod);
    public float GetModifiedDamage() => Mathf.Max(1f, (BaseDef.AttackDamage + DamageFlatMod) * DamageMultMod);
    public float GetModifiedAbilityPower() => Mathf.Max(0f, (BaseDef.AbilityPower + AbilityPowerFlatMod) * AbilityPowerMult);
}