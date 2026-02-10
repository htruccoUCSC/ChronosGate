using System;
using UnityEngine;

[Serializable]
public class Augment
{
    public Action Apply;



    public Augment(Action apply)
    {
        Apply = apply;
    }
}
