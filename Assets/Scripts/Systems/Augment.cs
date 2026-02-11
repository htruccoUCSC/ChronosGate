using System;
using UnityEngine;

[Serializable]
public class Augment
{
    public Action Apply;
    public string Name;


<<<<<<< Updated upstream
    public Augment(Action apply, string name)
    {

=======
    public Augment(Action apply, string Name )
    {
        this.Name = Name;
>>>>>>> Stashed changes
        Apply = apply;
        Name = name;
    }
}
