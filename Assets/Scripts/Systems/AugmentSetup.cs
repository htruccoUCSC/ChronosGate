using UnityEngine;

public class AugmentSetup : MonoBehaviour
{
    public TestAugment testAugment;
    public AugmentManager augmentManager;

    void Start()
    {
        // Create augment using the constructor that takes an Action
        Augment testAugmentWrapper = new Augment(() => testAugment.Test(),"Test Augment");

        // Add it to your AugmentManager
        augmentManager.augmentList.activeAugments.Add(testAugmentWrapper);
    }
}
