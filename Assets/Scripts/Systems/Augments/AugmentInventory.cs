using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AugmentInventory
{
    public List<Augment> ownedAugments = new List<Augment>();
    
    public void AddAugment(Augment augment)
    {
        ownedAugments.Add(augment);
    }
    
    public void RemoveAugment(Augment augment)
    {
        ownedAugments.Remove(augment);
    }
    
    public int GetAugmentCount()
    {
        return ownedAugments.Count;
    }
}