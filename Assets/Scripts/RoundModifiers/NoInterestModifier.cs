/// <summary>
/// MODIFIER: Recession
/// Effect: No interest is earned on gold this round.
///
/// HOW IT WORKS (context-only modifier):
///   OnRoundStart sets NoInterest=true in RoundModifierContext.
///   CurrencyManager.newRound() reads this flag before calling GetInterest():
///     if (!ctx.NoInterest) GetInterest();
///   Base income (8 gold per wave) is still awarded — only the interest calculation
///   on banked gold is suppressed.
///
/// BALANCE NOTES:
///   Punishes high-gold economies that rely on interest snowballing.
///   Low-gold players are barely affected, making this a soft economy check.
///   Can stack with TowerShopCostMult for a full economic crunch (future modifiers).
/// </summary>
public class NoInterestModifier
{
    public RoundModifier CreateModifier()
    {
        return new RoundModifier(
            name:            "Recession",
            description:     "No interest is earned on banked gold this round.",
            category:        RoundModifierCategory.EconomyDebuff,
            selectionWeight: 1.0f,

            onRoundStart: () =>
            {
                RoundModifierContext ctx = RoundModifierContext.Instance;
                if (ctx == null) return;

                ctx.NoInterest = true;
            },

            onRoundEnd: () => { }
        );
    }
}
