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
    public float OnhitModifier;

    public Action<float> OnHit;

    public Buff() { }

    public Buff(
        float AttackSpeedFlat,
        float AttackDamageFlat,
        float AttackSpeedMult,
        float AttackDamageMult,
        float AbilityPowerFlat,
        float duration,
        Action<float> OnHit = null,
        float AbilityPowerMult = 0,
        float OnhitModifier = 0f)
    {
        this.AttackSpeedFlat = AttackSpeedFlat;
        this.AttackDamageFlat = AttackDamageFlat;
        this.AttackSpeedMult = AttackSpeedMult;
        this.AttackDamageMult = AttackDamageMult;
        this.AbilityPowerFlat = AbilityPowerFlat;
        this.AbilityPowerMult = AbilityPowerMult;
        this.duration = duration;
        this.OnHit = OnHit;
        this.OnhitModifier = OnhitModifier;
    }
}
