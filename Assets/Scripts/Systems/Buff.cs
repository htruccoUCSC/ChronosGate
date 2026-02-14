using System;
using UnityEngine;
[System.Serializable]
public class Buff
{
    public float AttackSpeedFlat;
    public float AttackDamageFlat;
    public float AttackSpeedMult;
    public float AttackDamageMult;
    public float duration;
    public Action OnHit;

    public Buff() { }

    public Buff(
        float AttackSpeedFlat,
        float AttackDamageFlat,
        float AttackSpeedMult,
        float AttackDamageMult,
        float duration,
        Action OnHit )
    {
        this.AttackSpeedFlat = AttackSpeedFlat;
        this.AttackDamageFlat = AttackDamageFlat;
        this.AttackSpeedMult = AttackSpeedMult;
        this.AttackDamageMult = AttackDamageMult;
        this.duration = duration;
        this.OnHit = OnHit;
    }
}
