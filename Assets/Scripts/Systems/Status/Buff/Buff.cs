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
    public float RangeBuff;
    public float duration;
    public float OnhitModifier;
    public float OnKillModifier;
    public bool CalledFromAugment;
    public bool RefreshOnPlacement;

    public Action<float> OnHit;
    public Action<float> OnKill;

    public Buff() { }

    public Buff(
        float AttackSpeedFlat,
        float AttackDamageFlat,
        float AttackSpeedMult,
        float AttackDamageMult,
        float AbilityPowerFlat,
        float duration,
        Action<float> OnHit = null,
        Action<float> OnKill = null,
        float AbilityPowerMult = 0,
        float OnhitModifier = 0f,
        float OnKillModifier = 0f,
        float RangeBuff = 0f,
        bool CalledFromAugment = false,
        bool RefreshOnPlacement = false)
    {
        this.AttackSpeedFlat = AttackSpeedFlat;
        this.AttackDamageFlat = AttackDamageFlat;
        this.AttackSpeedMult = AttackSpeedMult;
        this.AttackDamageMult = AttackDamageMult;
        this.AbilityPowerFlat = AbilityPowerFlat;
        this.AbilityPowerMult = AbilityPowerMult;
        this.duration = duration;
        this.OnHit = OnHit;
        this.OnKill = OnKill;
        this.OnhitModifier = OnhitModifier;
        this.OnKillModifier = OnKillModifier;
        this.RangeBuff = RangeBuff;
        this.CalledFromAugment = CalledFromAugment;
        this.RefreshOnPlacement = RefreshOnPlacement;
    }
}
