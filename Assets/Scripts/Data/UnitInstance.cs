using UnityEngine;

public class UnitInstance : ScriptableObject
{
    // pointer to the base definition (shared data)
    public UnitDefinition BaseDef;

    // modifiable runtime values
    public float CurrentHP;
    public float CurrentMana;

    // modifiers from augments and shit
    public float DamageFlatMod = 0;
    public float SpeedMultMod = 1.0f;

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
    public float GetModifiedDamage() => (BaseDef.AttackDamage + DamageFlatMod) * SpeedMultMod;
    public float GetModifiedAttackSpeed() => BaseDef.AttackSpeed * SpeedMultMod;
}