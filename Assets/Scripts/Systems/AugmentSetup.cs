using UnityEngine;

public class AugmentSetup : MonoBehaviour
{
    public TestAugment testAugment;
    public ApeTogetherStrong ApeTogetherStrong;
    public ReserveAD reserveAD;
    public ToTheMoon toTheMoon;
    public ReserveAS reserveAS;
    public Renovations renovations;
    public AugmentManager augmentManager;
    public LuckyShot luckyShot;

    void Start()
    {
        // Create augment using the constructor that takes an Action
        Augment testAugmentWrapper = new Augment(() => testAugment.Test(),"Test Augment","if your a player whos see this oopsie");
        Augment ApeTogetherStrongWrapper = new Augment(() => ApeTogetherStrong.ApeTogetherStrongCall(),"Ape Together Strong","For each column of the board, if there is an prehistoric in that column, give all prehistoric in that column +20% Attack for each other prehistoric for the round.");
        Augment ReserveADWrapper = new Augment(() => reserveAD.ReserveADCall(),"Reserve AD","All towers gain +1 Attack for the round for every 5 gold you have.");
        Augment ReserveASWrapper = new Augment(() => reserveAS.ReserveASCall(),"Reserve AS","All towers gain +1 Attack Speed for the round for every 5 gold you have.");
        Augment ToTheMoonWrapper = new Augment(() => toTheMoon.ToTheMoonCall(),"To The Moon","Gain 8 gold for 3 rounds, increase your max interest by to 100.");
         Augment renovationsWrapper = new Augment(() => renovations.RenovationsCall(),"Renovations","Reduce board height by 2 and increase board width by 2. (same amounnt of enemies)");
            Augment LuckyShotWrapper = new Augment(() => luckyShot.LuckyShotCall(),"Lucky Shot","At the start of each round, all units have a 10% chance to immediately perform a basic attack.");

         augmentManager.AddInactiveAugment(ToTheMoonWrapper);
        augmentManager.AddInactiveAugment(testAugmentWrapper);
        augmentManager.AddInactiveAugment(ApeTogetherStrongWrapper);
        augmentManager.AddInactiveAugment(ReserveADWrapper);
        augmentManager.AddInactiveAugment(ReserveASWrapper);
        augmentManager.AddInactiveAugment(LuckyShotWrapper);    


        //ACTIVE FOR TESTING ONLY IF STARTED HERE, DELETE LATER
        augmentManager.AddActiveAugment(testAugmentWrapper);
        augmentManager.AddActiveAugment(ApeTogetherStrongWrapper);
        augmentManager.AddActiveAugment(ToTheMoonWrapper);
        augmentManager.AddActiveAugment(LuckyShotWrapper);    
       
    }
}
