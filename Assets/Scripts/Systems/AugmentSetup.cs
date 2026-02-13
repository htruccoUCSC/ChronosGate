using UnityEngine;

public class AugmentSetup : MonoBehaviour
{
    public TestAugment testAugment;
    public ApeTogetherStrong ApeTogetherStrong;
    public AugmentManager augmentManager;

    void Start()
    {
        // Create augment using the constructor that takes an Action
        Augment testAugmentWrapper = new Augment(() => testAugment.Test(),"Test Augment");
        Augment ApeTogetherStrongWrapper = new Augment(() => ApeTogetherStrong.ApeTogetherStrongCall(),"Ape Together Strong");

        augmentManager.AddInactiveAugment(testAugmentWrapper);
        augmentManager.AddInactiveAugment(ApeTogetherStrongWrapper);
        augmentManager.AddActiveAugment(testAugmentWrapper);
        // Add it to your AugmentManager
       
    }
}
