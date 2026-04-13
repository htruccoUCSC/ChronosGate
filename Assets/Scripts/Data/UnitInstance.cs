using UnityEngine;

public class UnitInstance : ScriptableObject
{
    // pointer to the base definition (shared data)
    public UnitDefinition BaseDef;
    public string Faction;
        public string Name;
    public float MaxHP;
    public float CurrentHP;
    public float CurrentMana;
    public float StartingMana;
    public int Level = 1;
    // modifiers from augments and shit
    public float DamageMultMod = 1.0f;
    public float DamageFlatMod = 0;
    public float SpeedFlatMod = 0;
    public float SpeedMultMod = 1.0f;
    public float BaseAbilityPower = 0;
    public float AbilityPowerFlatMod = 0;
    public float AbilityPowerMult=1.0f;
    public float RangeFlatMod = 0f;
    public string AttackType;


    // this is basically a constructor for creating runtime instances
    // needs to copy more data over and return modified values
    public static UnitInstance CreateRuntimeInstance(UnitDefinition def)
    {
        UnitInstance instance = ScriptableObject.CreateInstance<UnitInstance>();
        instance.BaseDef = def;
        instance.StartingMana = def.StartingMana;
        instance.CurrentMana = def.StartingMana;
        instance.Faction= def.Faction;
        instance.Name= def.Name;
        instance.Level = 1;
        instance.BaseAbilityPower = def.AbilityPower;
        instance.AbilityPowerFlatMod = 0f;
        instance.RangeFlatMod = 0f;
        instance.MaxHP = def.Health; 
        instance.CurrentHP = instance.MaxHP; 
        instance.AttackType = string.IsNullOrWhiteSpace(def.AttackType)
            ? def.AttackFunction.ToString()
            : def.AttackType;
        return instance;
    }

    public static UnitInstance CloneRuntimeInstance(UnitInstance source)
    {
        if (source == null)
        {
            return null;
        }

        UnitInstance instance = ScriptableObject.CreateInstance<UnitInstance>();
        instance.BaseDef = source.BaseDef;
        instance.Faction = source.Faction;
        instance.Name = source.Name;
        instance.MaxHP = source.MaxHP;
        instance.CurrentHP = source.CurrentHP;
        instance.CurrentMana = source.CurrentMana;
        instance.StartingMana = source.StartingMana;
        instance.Level = source.Level;
        instance.DamageMultMod = source.DamageMultMod;
        instance.DamageFlatMod = source.DamageFlatMod;
        instance.SpeedFlatMod = source.SpeedFlatMod;
        instance.SpeedMultMod = source.SpeedMultMod;
        instance.BaseAbilityPower = source.BaseAbilityPower;
        instance.AbilityPowerFlatMod = source.AbilityPowerFlatMod;
        instance.AbilityPowerMult = source.AbilityPowerMult;
        instance.RangeFlatMod = source.RangeFlatMod;
        instance.AttackType = source.AttackType;
        return instance;
    }

    // equations for modified stats with buffs
    //Clamp to prervent negative or zero values where it would break things
    public float GetModifiedAttackSpeed()
    {
        float baseSpeed = Mathf.Max(0.1f, BaseDef.AttackSpeed);
        float linearSpeed = Mathf.Max(0.1f, (BaseDef.AttackSpeed + SpeedFlatMod) * SpeedMultMod);

        if (linearSpeed <= baseSpeed)
        {
            return linearSpeed;
        }

        float normalizedBonus = (linearSpeed - baseSpeed) / baseSpeed;
        float logScaledSpeed = baseSpeed * (1f + Mathf.Log(1f + normalizedBonus));

        return Mathf.Max(0.1f, logScaledSpeed);
    }
    public float GetModifiedDamage() => Mathf.Max(1f, (BaseDef.AttackDamage + DamageFlatMod) * DamageMultMod);
    public float GetModifiedAbilityPower() => Mathf.Max(0f, (BaseDef.AbilityPower + AbilityPowerFlatMod) * AbilityPowerMult);
    public float GetModifiedRange() => Mathf.Max(0f, BaseDef.Range + RangeFlatMod);
}
