using System;
using UnityEngine;
[System.Serializable]
public class Buff
{
    public float AttackSpeedFlat;
    public float AttackDamageFlat;
    public float AttackSpeedMult;
    public float AttackDamageMult;

     public float AbilityPowerFlat;
    public float duration;
    public Action OnHit;

    public Buff() { }

    public Buff(
        float AttackSpeedFlat,
        float AttackDamageFlat,
        float AttackSpeedMult,
        float AttackDamageMult,
        float AbilityPowerFlat,
        float duration,
        Action OnHit )
    {
        this.AttackSpeedFlat = AttackSpeedFlat;
        this.AttackDamageFlat = AttackDamageFlat;
        this.AttackSpeedMult = AttackSpeedMult;
        this.AttackDamageMult = AttackDamageMult;
        this.AbilityPowerFlat = AbilityPowerFlat;
        this.duration = duration;
        this.OnHit = OnHit;
    }
}
