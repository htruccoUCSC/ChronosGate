using System;
using UnityEngine;

[Serializable]
public class Augment
{
    public Action Apply;
    public string Name;

    public Augment(Action apply, string name)
    {
        Apply = apply;
        Name = name;
    }
}
