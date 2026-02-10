using System;
using UnityEngine;

[Serializable]
public class Augment
{
    // Function that applies the augment to a unit
    public Action Apply;



    public Augment(Action apply)
    {
        Apply = apply;
    }
}
