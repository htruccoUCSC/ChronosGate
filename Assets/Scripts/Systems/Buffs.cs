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
        int duration,
        Action OnHit)
    {
        Buff buff = new Buff
        {
            AttackSpeedMult = attackSpeedMult,
            AttackSpeedFlat = attackSpeedFlat,
            AttackDamageFlat = attackDamageFlat,
            AttackDamageMult = attackDamageMult,
            duration = duration,
            OnHit  = OnHit
        };

        math.AddAttackDamage(unit.myData, attackDamageFlat);
        math.AddAttackSpeed(unit.myData, attackSpeedFlat);
        math.AddAttackMult(unit.myData, attackDamageMult);
        math.AddSpeedMult(unit.myData, attackSpeedMult);

        unit.AddTempBuff(buff);
    }

    public void AddRoundBuff(
        BaseUnit unit,
        float attackSpeedMult,
        float attackSpeedFlat,
        float attackDamageFlat,
        float attackDamageMult,
        Action OnHit)
    {
        Buff buff = new Buff
        {
            AttackSpeedMult = attackSpeedMult,
            AttackSpeedFlat = attackSpeedFlat,
            AttackDamageFlat = attackDamageFlat,
            AttackDamageMult = attackDamageMult,
            duration = 0f,
            OnHit = OnHit
        };

        math.AddAttackDamage(unit.myData, attackDamageFlat);
        math.AddAttackSpeed(unit.myData, attackSpeedFlat);
        math.AddAttackMult(unit.myData, attackDamageMult);
        math.AddSpeedMult(unit.myData, attackSpeedMult);

        unit.AddRoundBuff(buff);
    }

    public void RemoveTempBuff(BaseUnit unit, Buff buff)
    {
        math.SubAttackDamage(unit.myData, buff.AttackDamageFlat);
        math.SubAttackSpeed(unit.myData, buff.AttackSpeedFlat);
        math.SubAttackMult(unit.myData, buff.AttackDamageMult);
        math.SubSpeedMult(unit.myData, buff.AttackSpeedMult);

        unit.RemoveTempBuff(buff);
    }

    public void RemoveRoundBuff(BaseUnit unit, Buff buff)
    {
        math.SubAttackDamage(unit.myData, buff.AttackDamageFlat);
        math.SubAttackSpeed(unit.myData, buff.AttackSpeedFlat);
        math.SubAttackMult(unit.myData, buff.AttackDamageMult);
        math.SubSpeedMult(unit.myData, buff.AttackSpeedMult);

        unit.RemoveRoundBuff(buff);
    }
}
