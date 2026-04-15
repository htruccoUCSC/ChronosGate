/// <summary>
/// MODIFIER: Sluggish Arsenal
/// Effect: All towers attack 10% slower this round.
///
/// HOW IT WORKS (context-only modifier):
///   OnRoundStart sets TowerAttackSpeedMult=0.9 in RoundModifierContext.
///   UnitInstance.GetModifiedAttackSpeed() reads this as a final multiplier applied
///   AFTER the log-scaling curve, so the 10% reduction is clean and predictable
///   regardless of how many attack-speed augments a tower has.
///
///   Because GetModifiedAttackSpeed() is called every time BaseUnit.Update() recalculates
///   attackTimer, this modifier affects towers placed mid-round automatically — no
///   OnTowerPlaced implementation is needed.
///
/// WHY CONTEXT-ONLY IS SAFE HERE:
///   attack speed isn't stored as a field on the unit; it's recomputed each timer reset
///   from UnitInstance.GetModifiedAttackSpeed(). The context multiplier is read live,
///   so it applies and un-applies instantly when the context changes.
///
/// BALANCE NOTES:
///   A 10% attack speed reduction is a mild but consistent tax, especially on fast-firing
///   towers like Minigun. Combine with other TowerDebuff modifiers for harder rounds.
/// </summary>
public class ReducedTowerAttackSpeed
{
    public RoundModifier CreateModifier()
    {
        return new RoundModifier(
            name:            "Sluggish Arsenal",
            description:     "All towers attack 10% slower this round.",
            category:        RoundModifierCategory.TowerDebuff,
            selectionWeight: 1.1f,

            onRoundStart: () =>
            {
                RoundModifierContext ctx = RoundModifierContext.Instance;
                if (ctx == null) return;

                ctx.TowerAttackSpeedMult = 0.9f;
            },

            onRoundEnd: () => { }
        );
    }
}
