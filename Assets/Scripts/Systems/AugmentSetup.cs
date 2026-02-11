using UnityEngine;

public class AugmentSetup : MonoBehaviour
{
    public TestAugment testAugment;
    public AugmentManager augmentManager;

    void Start()
    {
         Augment testAugmentWrapper = new Augment(() => testAugment.Test(),"Test Augment");
         augmentManager.AddActiveAugment(testAugmentWrapper);
    }
    public void PurchaseTestAugment()
    {
        // Create augment using the constructor that takes an Action
        Augment testAugmentWrapper = new Augment(() => testAugment.Test(),"Test Augment");

        // Add it to your AugmentManager
        augmentManager.AddActiveAugment(testAugmentWrapper);
    }

}
