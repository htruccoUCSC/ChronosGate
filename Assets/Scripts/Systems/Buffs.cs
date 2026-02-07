using UnityEngine;

public class Buffs : MonoBehaviour
{
public BoardManager board;
public ModifyUnitStats math;

public void AddTempBuff(BaseUnit unit, float attackSpeedMult, float attackSpeedFlat, float attackDamageFlat, float attackDamageMult,int duration)
{
    Buff buff = new Buff
    {
        AttackSpeedMult = attackSpeedMult,
        AttackSpeedFlat = attackSpeedFlat,
        AttackDamageFlat = attackDamageFlat,
        AttackDamageMult = attackDamageMult,
        duration = duration
    };
    unit.AddTempBuff(buff);
}
public void RemoveTempBuff(BaseUnit unit, Buff buff)
{
    unit.RemoveTempBuff(buff);
}
public void RemoveRoundBuff(BaseUnit unit, Buff buff)
{
    unit.RemoveRoundBuff(buff);
}
public void AddRoundBuff(BaseUnit unit, float attackSpeedMult, float attackSpeedFlat, float attackDamageFlat, float attackDamageMult)
{
    Buff buff = new Buff
    {
        AttackSpeedMult = attackSpeedMult,
        AttackSpeedFlat = attackSpeedFlat,
        AttackDamageFlat = attackDamageFlat,
        AttackDamageMult = attackDamageMult,
        duration = 0f
    };
    unit.AddRoundBuff(buff);
}
}