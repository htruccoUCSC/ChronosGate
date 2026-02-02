using UnityEngine;

public class UnitInstance : ScriptableObject
{
    // pointer to the base definition (shared data)
    public UnitDefinition BaseDef;

    // modifiable runtime values
    public float MaxHP;
    public float CurrentMana;
    public float ManaCost;   
    // modifiers from augments and shit
    public float DamageFlatMod = 0;
    public float SpeedMultMod = 1.0f;
    public float Cost;
    public static UnitInstance CreateRuntimeInstance(UnitDefinition def)
    {
        UnitInstance instance = ScriptableObject.CreateInstance<UnitInstance>();
        instance.MaxHP = def.HP;
        //instance.BaseDef = def;
        instance.CurrentMana = def.StartingMana;
        instance.ManaCost = def.MaxMana;
        instance.Cost = def.Cost;
        // I forgot health but we'll add it later, is important LOL
        // instance.CurrentHP = def.BaseHealth; 
        return instance;
    }

    // equations for modified stats, we will change these later
    public float GetModifiedDamage() => (BaseDef.AttackDamage + DamageFlatMod) * SpeedMultMod;
    public float GetModifiedAttackSpeed() => BaseDef.AttackSpeed * SpeedMultMod;
}