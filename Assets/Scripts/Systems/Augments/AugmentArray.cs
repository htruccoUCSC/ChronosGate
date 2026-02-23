using System.Collections.Generic;
using System;

[Serializable]
public class AugmentList
{
    public List<Augment> activeAugments = new List<Augment>();
    public List<Augment> inactiveAugments = new List<Augment>();
}
