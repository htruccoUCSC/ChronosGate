using System;
using UnityEngine;

[Serializable]
public class Augment
{
    public Action Apply;
    public Action OnAcquire;
    public string Name;
    public string Description;
    public bool ApplyImmediatelyOnAcquire;
    public bool ApplyOnRoundStart;

    public Augment(Action apply, string name, string description, bool applyImmediatelyOnAcquire = false, bool applyOnRoundStart = true, Action onAcquire = null)
    {

        Apply = apply;
        OnAcquire = onAcquire;
        Name = name;
        Description = description;
        ApplyImmediatelyOnAcquire = applyImmediatelyOnAcquire;
        ApplyOnRoundStart = applyOnRoundStart;
    }
}
