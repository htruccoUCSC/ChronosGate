using System;
using UnityEngine;
[System.Serializable]
public class Buff
{
    public float AttackSpeedFlat;
    public float AttackDamageFlat;
    public float AttackSpeedMult;
    public float AttackDamageMult;
    public float AbilityPowerMult;
     public float AbilityPowerFlat;
    public float duration;
    public Action OnHit;

    public Buff() { }
    // IK ITS UNORGANIZED BUT I ODNT WANT TO REFACTOR!!!! YOU CANT MAKE ME
    public Buff(
        float AttackSpeedFlat,
        float AttackDamageFlat,
        float AttackSpeedMult,
        float AttackDamageMult,
        float AbilityPowerFlat,
        float duration,
        Action OnHit ,
        float AbilityPowerMult = 0)
    {
        this.AttackSpeedFlat = AttackSpeedFlat;
        this.AttackDamageFlat = AttackDamageFlat;
        this.AttackSpeedMult = AttackSpeedMult;
        this.AttackDamageMult = AttackDamageMult;
        this.AbilityPowerFlat = AbilityPowerFlat;
        this.AbilityPowerMult = AbilityPowerMult;
        this.duration = duration;
        this.OnHit = OnHit;
    }
}
