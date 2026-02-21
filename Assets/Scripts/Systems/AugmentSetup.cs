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
    public Unique Unique;
    public LongGame longGame;

    public OldSchool oldSchool;

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
        Augment UniqueWrapper = new Augment(() => Unique.UniqueCall(),"Unique","If a unit is surrounded by different eras gain 3x mult on attack speed and damage with 50 AP");
        Augment OldSchoolWrapper = new Augment(() => oldSchool.OldSchoolCall(),"OldSchool","All units gain 20% Attack Speed for the round.");
        Augment LongGameWrapper = new Augment(() => longGame.LongGameCall(),"LongGame","All units gain 0.1x ability power for  round that have passed , Future Units gain a 50 AP");

        augmentManager.AddInactiveAugment(ToTheMoonWrapper);
        augmentManager.AddInactiveAugment(testAugmentWrapper);
        augmentManager.AddInactiveAugment(ApeTogetherStrongWrapper);
        augmentManager.AddInactiveAugment(ReserveADWrapper);
        augmentManager.AddInactiveAugment(ReserveASWrapper);
        augmentManager.AddInactiveAugment(UniqueWrapper);
        augmentManager.AddInactiveAugment(OldSchoolWrapper);      
        augmentManager.AddInactiveAugment(LongGameWrapper);    

        //ACTIVE FOR TESTING ONLY IF STARTED HERE, DELETE LATER
        // augmentManager.AddActiveAugment(testAugmentWrapper);
        // augmentManager.AddActiveAugment(ApeTogetherStrongWrapper);
        // augmentManager.AddActiveAugment(ToTheMoonWrapper);
        // augmentManager.AddActiveAugment(LuckyShotWrapper);  
        //  augmentManager.AddActiveAugment(UniqueWrapper);
         augmentManager.AddActiveAugment(OldSchoolWrapper);      
       // augmentManager.AddActiveAugment(LongGameWrapper);
    }
}
