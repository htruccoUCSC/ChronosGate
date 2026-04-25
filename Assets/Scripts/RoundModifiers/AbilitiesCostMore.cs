/// <summary>
/// MODIFIER: Mana Tax
/// Effect: Tower abilities cost 30% more mana to trigger this round.
///
/// HOW IT WORKS (context-only modifier):
///   OnRoundStart sets TowerAbilityCostMult=1.3 in RoundModifierContext.
///   BaseUnit.Update() reads this when checking:
///     if (CurrentMana >= BaseDef.AbilityManaCost * ctx.TowerAbilityCostMult)
///   So every tower needs 30% more mana accumulated before casting.
///   Mana gain per shot is unchanged — it just takes more shots to reach the threshold.
///
/// BALANCE NOTES:
///   Punishes ability-heavy compositions (Wizard, FirePriestess, etc.) more than
///   basic-attack towers. Pairs well with TowerManaGainMult reductions for harsher debuffs.
/// </summary>
public class AbilitiesCostMore
{
    public RoundModifier CreateModifier()
    {
        return new RoundModifier(
            name:            "Mana Tax",
            description:     "Tower abilities require 30% more mana to trigger this round.",
            category:        RoundModifierCategory.TowerDebuff,
            selectionWeight: 1.0f,

            onRoundStart: () =>
            {
                RoundModifierContext ctx = RoundModifierContext.Instance;
                if (ctx == null) return;

                ctx.TowerAbilityCostMult = 1.3f;
            },

            onRoundEnd: () => { }
        );
    }
}
