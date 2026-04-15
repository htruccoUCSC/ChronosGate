/// <summary>
/// MODIFIER: Blitzkrieg
/// Effect: Enemies move twice as fast but spawn with half their normal max health.
///
/// HOW IT WORKS (context-only modifier — no direct tower changes):
///   OnRoundStart sets EnemySpeedMult=2.0 and EnemyHealthMult=0.5 in RoundModifierContext.
///   These are read inside BaseEnemy.ApplyWaveScaling() at the moment each enemy spawns,
///   so every enemy in the round is affected automatically without any extra code.
///   OnRoundEnd is empty — Context.Reset() in RoundModifierManager handles cleanup.
///
/// BALANCE NOTES:
///   Fast enemies reach towers sooner, reducing reaction time and ability usage windows.
///   Half health partially compensates by making them easier to burst down.
///   Net effect: favors high burst-damage and close-range towers over sustained DPS.
/// </summary>
public class EnemiesDoubleSpeedHalfHealth
{
    public RoundModifier CreateModifier()
    {
        return new RoundModifier(
            name:            "Blitzkrieg",
            description:     "Enemies move twice as fast, but arrive with half their normal health.",
            category:        RoundModifierCategory.EnemyBuff,
            selectionWeight: 1.2f,

            onRoundStart: () =>
            {
                RoundModifierContext ctx = RoundModifierContext.Instance;
                if (ctx == null) return;

                ctx.EnemySpeedMult  = 2.0f;
                ctx.EnemyHealthMult = 0.5f;
            },

            // Context.Reset() is called automatically by RoundModifierManager after
            // all OnRoundEnd callbacks, so context-only modifiers leave this empty.
            onRoundEnd: () => { }
        );
    }
}
