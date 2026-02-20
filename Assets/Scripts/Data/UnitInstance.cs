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
        instance.AbilityPowerFlatMod = instance.BaseAbilityPower;
        instance.MaxHP = def.Health; 
        instance.CurrentHP = instance.MaxHP; 
        return instance;
    }

    // equations for modified stats, we will change these later
    public float GetModifiedDamage() => (BaseDef.AttackDamage + DamageFlatMod) * DamageMultMod;
    public float GetModifiedAttackSpeed() => (BaseDef.AttackSpeed + SpeedFlatMod) * SpeedMultMod;
    public float GetModifiedAbilityPower() => (BaseDef.AbilityPower + AbilityPowerFlatMod) * AbilityPowerMult;
}