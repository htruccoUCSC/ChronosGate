using UnityEngine;
using System;
public class Buffs : MonoBehaviour
{
    public BoardManager board;
    public ModifyUnitStats math;

    public void AddTempBuff(
        BaseUnit unit,
        float attackSpeedMult,
        float attackSpeedFlat,
        float attackDamageFlat,
        float attackDamageMult,
        float abilityPowerFlat,
        float abilityPowerMult,
        int duration,
        Action<float> OnHit,
        float onHitModifier = 0f)
    {
        Buff buff = new Buff
        {
            AttackSpeedMult = attackSpeedMult,
            AttackSpeedFlat = attackSpeedFlat,
            AttackDamageFlat = attackDamageFlat,
            AttackDamageMult = attackDamageMult,
            AbilityPowerFlat = abilityPowerFlat,

            duration = duration,
            OnHit = OnHit,
            OnhitModifier = onHitModifier,
            AbilityPowerMult=abilityPowerMult,
        };

        math.AddAttackDamage(unit.myData, attackDamageFlat);
        math.AddAttackSpeed(unit.myData, attackSpeedFlat);
        math.AddAttackMult(unit.myData, attackDamageMult);
        math.AddSpeedMult(unit.myData, attackSpeedMult);
        math.AddAbilityPower(unit.myData, abilityPowerFlat);
        math.AddAbilityPowerMult(unit.myData,abilityPowerMult);
        unit.AddTempBuff(buff);
        Debug.Log($"[Buffs.AddTempBuff] {unit.name} SpeedFlatMod: {unit.myData.SpeedFlatMod}, SpeedMultMod: {unit.myData.SpeedMultMod}");
    }

    public void AddRoundBuff(
        BaseUnit unit,
        float attackSpeedMult,
        float attackSpeedFlat,
        float attackDamageFlat,
        float attackDamageMult,
        float abilityPowerFlat,
        float abilityPowerMult,
        Action<float> OnHit,
        float onHitModifier = 0f)
    {
        Buff buff = new Buff
        {
            AttackSpeedMult = attackSpeedMult,
            AttackSpeedFlat = attackSpeedFlat,
            AttackDamageFlat = attackDamageFlat,
            AttackDamageMult = attackDamageMult,
            AbilityPowerFlat = abilityPowerFlat,

            duration = 0f,
            OnHit = OnHit,
            OnhitModifier = onHitModifier,
            AbilityPowerMult=abilityPowerMult,
        };

        math.AddAttackDamage(unit.myData, attackDamageFlat);
        math.AddAttackSpeed(unit.myData, attackSpeedFlat);
        math.AddAttackMult(unit.myData, attackDamageMult);
        math.AddSpeedMult(unit.myData, attackSpeedMult);
        math.AddAbilityPower(unit.myData, abilityPowerFlat);
        math.AddAbilityPowerMult(unit.myData,abilityPowerMult);

        unit.AddRoundBuff(buff);
    }

    public void RemoveTempBuff(BaseUnit unit, Buff buff)
    {
        math.SubAttackDamage(unit.myData, buff.AttackDamageFlat);
        math.SubAttackSpeed(unit.myData, buff.AttackSpeedFlat);
        math.SubAttackMult(unit.myData, buff.AttackDamageMult);
        math.SubSpeedMult(unit.myData, buff.AttackSpeedMult);
        math.SubAbilityPower(unit.myData, buff.AbilityPowerFlat);
        math.SubAbilityPowerMult(unit.myData,buff.AbilityPowerMult);

        unit.RemoveTempBuff(buff);
        Debug.Log($"[Buffs.RemoveTempBuff] {unit.name} SpeedFlatMod: {unit.myData.SpeedFlatMod}, SpeedMultMod: {unit.myData.SpeedMultMod}");
    }

    public void RemoveRoundBuff(BaseUnit unit, Buff buff)
    {
        math.SubAttackDamage(unit.myData, buff.AttackDamageFlat);
        math.SubAttackSpeed(unit.myData, buff.AttackSpeedFlat);
        math.SubAttackMult(unit.myData, buff.AttackDamageMult);
        math.SubSpeedMult(unit.myData, buff.AttackSpeedMult);
        math.SubAbilityPower(unit.myData, buff.AbilityPowerFlat);
         math.SubAbilityPowerMult(unit.myData,buff.AbilityPowerMult);

        unit.RemoveRoundBuff(buff);
    }
}
