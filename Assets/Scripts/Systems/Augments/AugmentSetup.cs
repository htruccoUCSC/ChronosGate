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
    public WildFire wildFire;
    public Exposure exposure;
    public LucrativeProfession lucrativeProfession;
    public Clastrophobic clastrophobic;
    public OldSchool oldSchool;
    public Rally rally;
    public Scoped scoped;
    public Overwork overwork;
    public QuickStart quickStart;
    public Diversity diversity;
    public AbilityPowerOnCast abilityPowerOnCast;
    public TimeSpiral timeSpiral;

    void Start()
    {
        // Create augment using the constructor that takes an Action
        Augment testAugmentWrapper = new Augment(() => testAugment.Test(),"Test Augment","if your a player whos see this oopsie");
        Augment ApeTogetherStrongWrapper = new Augment(() => ApeTogetherStrong.ApeTogetherStrongCall(),"Ape Together Strong","For each column of the board, if there is an prehistoric in that column, give all prehistoric in that column +20% Attack for each other prehistoric for the round.", update: true);
        Augment ReserveADWrapper = new Augment(() => reserveAD.ReserveADCall(),"Reserve AD","All towers gain +1 Attack for the round for every 5 gold you have.", update: true);
        Augment ReserveASWrapper = new Augment(() => reserveAS.ReserveASCall(),"Reserve AS","All towers gain +1 Attack Speed for the round for every 5 gold you have.", update: true);
        Augment ToTheMoonWrapper = new Augment(() => toTheMoon.ToTheMoonCall(),"To The Moon","Gain 8 gold for 3 rounds, increase your max interest by to 100.", applyImmediatelyOnAcquire: true, 
            applyOnRoundStart: true);
        Augment RenovationsWrapper = new Augment(
            () => renovations.RenovationsCall(),
            "Renovations",
            "Reduce board height by 2 and increase board width by 2. (same amounnt of enemies)",
            applyImmediatelyOnAcquire: true,
            applyOnRoundStart: false,
            update: false);
        Augment LuckyShotWrapper = new Augment(() => luckyShot.LuckyShotCall(),"Lucky Shot","All units have a 10% to double hit", update: true);
        Augment UniqueWrapper = new Augment(() => Unique.UniqueCall(),"Unique","If a unit is surrounded by different eras gain 3x mult on attack speed and damage with 50 AP", update: true);
        Augment OldSchoolWrapper = new Augment(() => oldSchool.OldSchoolCall(),"OldSchool","Medieval, Prehistoric, and Mystic units gain 30 AP for each surrounding unit of that type" ,update: true);
        Augment LongGameWrapper = new Augment(() => longGame.LongGameCall(),"LongGame","All units gain 0.1x ability power for rounds since this augment was acquired, Future Units gain 50 AP.", onAcquire: () => longGame.MarkAcquired(), update: true);
        Augment WildfireWrapper = new Augment(() => wildFire.WildFireCall(),"Wildfire","All units apply fire onHit" ,update: true);
        Augment ExposureWrapper = new Augment(() => exposure.ExposureCall(),"Exposure","All units 5% damage amp per shot" ,update: true);
        Augment LucrativeProfessionWrapper = new Augment(() => lucrativeProfession.LucrativeProfessionCall(),"Lucrative Profession","bounty hunters generate 1 gold for each 5 kills" ,update: true);
        Augment ClastrophobicWrapper = new Augment(() => clastrophobic.ClastrophobicCall(),"Clastrophobic","All units gain +current round in AS,AD,AP for each empty column" ,update: true);
        Augment RallyWrapper = new Augment(() => rally.RallyCall(),"Rally","Melee units gain +1 range for each concurrent melee unit infront of them" ,update: true);
        Augment ScopedWrapper = new Augment(() => scoped.ScopedCall(),"Scoped","Units without a MeleeAttackBehavior gain +1 range for the round." ,update: true);
        Augment OverworkWrapper = new Augment(() => overwork.OverworkCall(),"Overwork","All units deal 3x damage but take 1 damage on hit." ,update: true);
        Augment QuickStartWrapper = new Augment(() => quickStart.QuickStartCall(),"Quick Start","All units start the round with their ability ready." );
        Augment DiversityWrapper = new Augment(() => diversity.DiversityCall(),"Diversity","Rows with 3+ distinct factions gain +10% attack speed for the round." ,update: true);
        Augment AbilityPowerOnCastWrapper = new Augment(() => abilityPowerOnCast.AbilityPowerOnCastCall(),"AP","Each ability cast grants +1 ability power to all towers for the round." ,update: true);
        Augment TimeSpiralWrapper = new Augment(() => timeSpiral.TimeSpiralCall(),"Time Spiral","Every second, all units gain +2 attack damage and +2 ability power for the round." ,update: true);

        augmentManager.AddInactiveAugment(LucrativeProfessionWrapper);
        augmentManager.AddInactiveAugment(ToTheMoonWrapper);
        augmentManager.AddInactiveAugment(ApeTogetherStrongWrapper);
        augmentManager.AddInactiveAugment(ReserveADWrapper);
        augmentManager.AddInactiveAugment(ReserveASWrapper);
        augmentManager.AddInactiveAugment(UniqueWrapper);
        augmentManager.AddInactiveAugment(OldSchoolWrapper);      
        augmentManager.AddInactiveAugment(LongGameWrapper);    
        augmentManager.AddInactiveAugment(LuckyShotWrapper);
        augmentManager.AddInactiveAugment(RenovationsWrapper);
        augmentManager.AddInactiveAugment(WildfireWrapper);
        augmentManager.AddInactiveAugment(ExposureWrapper);
        augmentManager.AddInactiveAugment(ClastrophobicWrapper);
        augmentManager.AddInactiveAugment(RallyWrapper);
        augmentManager.AddInactiveAugment(ScopedWrapper);
        augmentManager.AddInactiveAugment(OverworkWrapper);
        augmentManager.AddInactiveAugment(QuickStartWrapper);
        augmentManager.AddInactiveAugment(DiversityWrapper);
        augmentManager.AddInactiveAugment(AbilityPowerOnCastWrapper);
        augmentManager.AddInactiveAugment(TimeSpiralWrapper);
        //ACTIVE FOR TESTING ONLY IF STARTED HERE, DELETE LATER
        // augmentManager.AddActiveAugment(TimeSpiralWrapper);
        // augmentManager.AddActiveAugment(ApeTogetherStrongWrapper);
        // augmentManager.AddActiveAugment(ToTheMoonWrapper);
        // augmentManager.AddActiveAugment(LuckyShotWrapper);  
        //  augmentManager.AddActiveAugment(UniqueWrapper);
         //augmentManager.AddActiveAugment(OldSchoolWrapper);      
       // augmentManager.AddActiveAugment(LongGameWrapper);
       //augmentManager.AddActiveAugment(LucrativeProfessionWrapper);
       //augmentManager.AddActiveAugment(RallyWrapper);
    }
}
