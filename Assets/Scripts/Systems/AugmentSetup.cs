using UnityEngine;

public class AugmentSetup : MonoBehaviour
{
    public TestAugment testAugment;
    public ApeTogetherStrong ApeTogetherStrong;
    public ReserveAD reserveAD;
    public ReserveAS reserveAS;
    public AugmentManager augmentManager;

    void Start()
    {
        // Create augment using the constructor that takes an Action
        Augment testAugmentWrapper = new Augment(() => testAugment.Test(),"Test Augment","if your a player whos see this oopsie");
        Augment ApeTogetherStrongWrapper = new Augment(() => ApeTogetherStrong.ApeTogetherStrongCall(),"Ape Together Strong","For each column of the board, if there is an prehistoric in that column, give all prehistoric in that column +20% Attack for each other prehistoric for the round.");
        Augment ReserveADWrapper = new Augment(() => reserveAD.ReserveADCall(),"Reserve AD","All towers gain +1 Attack for the round for every 5 gold you have.");
        Augment ReserveASWrapper = new Augment(() => reserveAS.ReserveASCall(),"Reserve AS","All towers gain +1 Attack Speed for the round for every 5 gold you have.");

        augmentManager.AddInactiveAugment(testAugmentWrapper);
        augmentManager.AddInactiveAugment(ApeTogetherStrongWrapper);
        augmentManager.AddInactiveAugment(ReserveADWrapper);
        augmentManager.AddInactiveAugment(ReserveASWrapper);
    }
}
