
using System;
using UnityEngine;
[System.Serializable]
public class Debuff
{
    public float amountOfStacks;
    public float duration;
    public Action<float> func;


    public Debuff() { }
    public Debuff(
        float amountOfStacks,
        Action<float> func,
        float duration)
    {
       
        this.amountOfStacks = amountOfStacks;
        this.duration = duration;
        this.func = func;
    }
}
