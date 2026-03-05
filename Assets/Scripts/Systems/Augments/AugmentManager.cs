using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
//using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public AugmentList augmentList = new AugmentList();
    private List<Augment> augmentInventory = new List<Augment>(); // NEW

    // NEW: Acquire an augment and add to inventory
    public void AcquireAugment(Augment newAugment)
    {
        augmentInventory.Add(newAugment);
    }

    // NEW: Get inventory for UI display
    public List<Augment> GetAugmentInventory()
    {
        return augmentInventory;
    }

    public void ApplyAllActiveAugments()
    {
        for (int i = 0; i < augmentList.activeAugments.Count; i++)
        {
            augmentList.activeAugments[i].Apply();
        }
    }
    public void AddActiveAugment(Augment newAugment)
    {
        augmentList.activeAugments.Add(newAugment);
    }
    public void AddInactiveAugment(Augment newAugment)
    {
        augmentList.inactiveAugments.Add(newAugment);
    }
        public void RemoveActiveAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.activeAugments.Count)
        {
            augmentList.activeAugments.RemoveAt(index);
        }
    }
            public void RemoveInactiveAugmentByIndex(int index)
    {
        if (index >= 0 && index < augmentList.activeAugments.Count)
        {
            augmentList.inactiveAugments.RemoveAt(index);
        }
    }
}

