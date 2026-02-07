using UnityEngine;

public class UnitInstance : ScriptableObject
{
    // pointer to the base definition (shared data)
    public UnitDefinition BaseDef;

    // modifiable runtime values
    public float CurrentHP;
    public float CurrentMana;

    // modifiers from augments and shit
    public float DamageMultMod = 1.0f;
    public float DamageFlatMod = 0;
    public float SpeedFlatMod = 0;
    public float SpeedMultMod = 1.0f;

    // this is basically a constructor for creating runtime instances
    // needs to copy more data over and return modified values
    public static UnitInstance CreateRuntimeInstance(UnitDefinition def)
    {
        UnitInstance instance = ScriptableObject.CreateInstance<UnitInstance>();
        instance.BaseDef = def;
        instance.CurrentMana = def.StartingMana;
        // I forgot health but we'll add it later, is important LOL
        // instance.CurrentHP = def.BaseHealth; 
        return instance;
    }

    // equations for modified stats, we will change these later
    public float GetModifiedDamage() => (BaseDef.AttackDamage + DamageFlatMod) * DamageMultMod;
    public float GetModifiedAttackSpeed() => (BaseDef.AttackSpeed + SpeedFlatMod) * SpeedMultMod;
}