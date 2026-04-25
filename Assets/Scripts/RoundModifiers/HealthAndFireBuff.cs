using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MODIFIER: Bulwark Flame
/// Effect: All towers gain 25% more max health. Fire damage deals 50% more damage per tick.
///
/// HOW IT WORKS (MIXED: direct tower modification + context):
///
///   FIRE DAMAGE (context-only):
///     OnRoundStart sets FireDamageMult=1.5 in RoundModifierContext.
///     BaseEnemy.ApplyFire() reads this when calculating tick damage.
///     Context.Reset() cleans it up — no manual reversal needed.
///
///   MAX HEALTH (direct modification):
///     OnRoundStart iterates all current towers via RoundModifierManager.ForEachTower()
///     and adds a 25% HP delta to both MaxHP and CurrentHP (towers are also healed).
///     The exact delta added to each tower is stored in 'appliedHpDeltas' so that
///     OnRoundEnd can reverse it precisely, even if other effects changed HP mid-round.
///
///     OnTowerPlaced applies the same HP buff to any tower placed mid-round.
///
/// CLEANUP ORDER:
///   OnRoundEnd reverses HP changes on all tracked towers first, then clears the dictionary.
///   Context.Reset() fires after all OnRoundEnd callbacks (handled by RoundModifierManager).
///
/// BALANCE NOTES:
///   The health buff makes towers more durable but does not persist between rounds
///   (health resets in NewRound.BaseUnitNewRoundCalls). The fire buff benefits
///   fire-applying towers (WildFire augment, FirePriestess, any future fire units).
/// </summary>
public class HealthAndFireBuff
{
    // Stores the exact HP delta added to each tower so we can reverse it cleanly.
    // Key = tower, Value = how much MaxHP was added. Cleared each round.
    private readonly Dictionary<BaseUnit, float> appliedHpDeltas = new Dictionary<BaseUnit, float>();

    public RoundModifier CreateModifier()
    {
        return new RoundModifier(
            name:            "Bulwark Flame",
            description:     "All towers gain 25% more max health. Fire damage is increased by 50%.",
            category:        RoundModifierCategory.TowerBuff,
            selectionWeight: 0.9f,

            onRoundStart: () =>
            {
                appliedHpDeltas.Clear();

                // Set fire damage boost via context (read by BaseEnemy.ApplyFire)
                RoundModifierContext ctx = RoundModifierContext.Instance;
                if (ctx != null)
                    ctx.FireDamageMult = 1.5f;

                // Apply +25% max HP directly to all towers currently on the board
                RoundModifierManager.Instance?.ForEachTower(ApplyHealthBuff);
            },

            onRoundEnd: () =>
            {
                // Reverse the HP changes for every tower that was buffed.
                // Iterate by key so we only touch towers this modifier actually changed.
                foreach (KeyValuePair<BaseUnit, float> entry in appliedHpDeltas)
                {
                    BaseUnit unit  = entry.Key;
                    float    delta = entry.Value;

                    // Guard against towers destroyed mid-round
                    if (unit == null || unit.IsDead || unit.myData == null) continue;

                    unit.myData.MaxHP -= delta;

                    // Clamp current HP so it never exceeds the restored max
                    unit.myData.CurrentHP = Mathf.Min(unit.myData.CurrentHP, unit.myData.MaxHP);
                }

                appliedHpDeltas.Clear();
                // FireDamageMult is reset automatically by Context.Reset() after this callback
            },

            // Apply the HP buff to towers placed mid-round so they aren't skipped
            onTowerPlaced: (unit) =>
            {
                if (unit == null || unit.IsDead || unit.myData == null) return;
                ApplyHealthBuff(unit);
            }
        );
    }

    /// <summary>
    /// Adds 25% of the tower's current MaxHP to both MaxHP and CurrentHP,
    /// then records the delta for later reversal.
    /// If the same tower is somehow buffed twice (e.g. mid-round re-placement),
    /// the dictionary overwrites the stored delta — intentional, not a bug.
    /// </summary>
    private void ApplyHealthBuff(BaseUnit unit)
    {
        if (unit == null || unit.IsDead || unit.myData == null) return;

        float delta = unit.myData.MaxHP * 0.25f;
        unit.myData.MaxHP     += delta;
        unit.myData.CurrentHP += delta; // Also heal the tower by the added amount

        // Overwrite if somehow applied twice (protects against double-placement edge case)
        appliedHpDeltas[unit] = delta;
    }
}
