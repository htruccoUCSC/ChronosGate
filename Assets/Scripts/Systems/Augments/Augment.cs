using System;
using UnityEngine;

[Serializable]
public class Augment
{
    public Action Apply;
    public string Name;
    public string Description;

    public Augment(Action apply, string name,string description)
    {

        Apply = apply;
        Name = name;
        Description = description;
    }
}
