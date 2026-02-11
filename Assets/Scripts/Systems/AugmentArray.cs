using System.Collections.Generic;
using System;

[Serializable]
public class AugmentList
{
    public List<Augment> activeAugments = new List<Augment>();
    
    public List<Augment>RoundStartAugments = new List<Augment>();
    
    public List<Augment> ShopAugments = new List<Augment>();
    
    public List<Augment> AllAugments = new List<Augment>();

}
