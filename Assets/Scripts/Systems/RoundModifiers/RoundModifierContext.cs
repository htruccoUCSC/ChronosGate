using UnityEngine;

/// <summary>
/// Singleton data bag that holds all active round modifier multipliers and flags.
///
/// DESIGN PHILOSOPHY:
///   Existing systems (BaseEnemy, BaseUnit, CurrencyManager, etc.) read from this class
///   rather than knowing anything about the modifier system directly. This means adding a
///   new modifier never requires changes to existing game code — just add a field here,
///   read it in the relevant system, and set it in the modifier's OnRoundStart action.
///
///   All multipliers default to 1.0 (neutral — no change) and all booleans default to false.
///   RoundModifierManager.RemoveModifiers() calls Reset() at the end of every cycle.
///
/// UNITY SETUP:
///   Add this component to the same persistent GameObject as GameLoopManager
///   (e.g. "GameManager" or "Systems"). It self-initializes as a singleton.
///   No Inspector wiring required.
/// </summary>
public class RoundModifierContext : MonoBehaviour
{
    public static RoundModifierContext Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // =========================================================================
    // TOWER (PLAYER) STATS
    // These are read inside UnitInstance.GetModified*() so they automatically
    // apply to every tower on the board, including ones placed mid-round.
    // =========================================================================

    /// <summary>
    /// Multiplies all tower basic attack damage.
    /// Read by UnitInstance.GetModifiedDamage().
    /// </summary>
    public float TowerAttackDamageMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies all tower attack speed (attacks per second).
    /// Applied after the log-scaling curve in UnitInstance.GetModifiedAttackSpeed(),
    /// so diminishing returns still apply to augment bonuses but this modifier is a
    /// clean flat multiplier on top of the final result.
    /// </summary>
    public float TowerAttackSpeedMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies all tower ability power.
    /// Read by UnitInstance.GetModifiedAbilityPower().
    /// </summary>
    public float TowerAbilityPowerMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies all tower range.
    /// Read by UnitInstance.GetModifiedRange().
    /// </summary>
    public float TowerRangeMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the mana threshold required to trigger an ability.
    /// A value of 1.3 means abilities need 30% more mana before they fire.
    /// Read in BaseUnit.Update() where CurrentMana is compared to BaseDef.AbilityManaCost.
    /// </summary>
    public float TowerAbilityCostMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies how much mana towers gain per shot (base: 10 mana/shot).
    /// A value of 0.7 means towers build ability charge 30% slower.
    /// Read in BaseUnit.Update() when manaPerShot is added to CurrentMana.
    /// </summary>
    public float TowerManaGainMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies tower max HP. This field is for reference/tracking only —
    /// the HealthAndFireBuff modifier applies HP changes directly to myData.MaxHP
    /// rather than reading this from a getter, since MaxHP is not behind a getter.
    /// The modifier's OnRoundEnd reverses those direct changes.
    /// </summary>
    public float TowerMaxHealthMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the travel speed of all spawned projectiles.
    /// TODO: Hook into Projectile.Setup() (the 'speed' field) once projectile speed
    /// tuning is needed for a modifier. Not currently read by any system.
    /// </summary>
    public float ProjectileSpeedMult { get; set; } = 1f;

    // =========================================================================
    // ENEMY STATS
    // Read inside BaseEnemy.ApplyWaveScaling(), which runs on every enemy spawn.
    // Applied after the standard wave scaling, so modifiers stack on top of
    // the normal difficulty ramp rather than competing with it.
    // =========================================================================

    /// <summary>
    /// Multiplies enemy max health after wave scaling.
    /// Read in BaseEnemy.ApplyWaveScaling().
    /// </summary>
    public float EnemyHealthMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies enemy move speed after wave scaling.
    /// Read in BaseEnemy.ApplyWaveScaling().
    /// </summary>
    public float EnemySpeedMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies enemy contact damage-per-second (scales per-hit damage value).
    /// Read in BaseEnemy.ApplyWaveScaling().
    /// Combine with EnemyAttackSpeedMult for fine-grained control:
    ///   EnemyDamageMult=0.5 + EnemyAttackSpeedMult=2 → same total DPS but
    ///   faster, weaker hits (useful once enemies have a discrete attack system).
    /// </summary>
    public float EnemyDamageMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies how rapidly enemies deal contact damage (scales attack cadence).
    /// Currently applies to damagePerSecond alongside EnemyDamageMult — this field
    /// is architecturally separated so that when enemies gain a discrete hit system,
    /// EnemyAttackSpeedMult can move there without changing any modifier code.
    /// Read in BaseEnemy.ApplyWaveScaling().
    /// </summary>
    public float EnemyAttackSpeedMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies all DamageAmp stacks applied to enemies via BaseUnit.ApplyAmp().
    /// Higher values make the damage amplification debuff stronger per application.
    /// </summary>
    public float EnemyDamageAmpMult { get; set; } = 1f;

    // =========================================================================
    // STATUS EFFECTS & ELEMENTAL MODIFIERS
    // Read at the call sites where debuffs are applied, so every source of that
    // debuff type (towers, augments, abilities) benefits automatically.
    //
    // SPLIT RESPONSIBILITY:
    //   Damage/strength multipliers → applied at the ENEMY side (BaseEnemy.ApplyFire,
    //     ApplyAmp, ApplySlow) where the effect actually does something.
    //   Duration/stack multipliers  → applied at the TOWER side (BaseUnit.ApplyFire,
    //     ApplySlow, ApplyAmp) when building the debuff, so the debuff object itself
    //     is constructed with the correct modified values.
    // =========================================================================

    // --- FIRE / BURN ---------------------------------------------------------

    /// <summary>
    /// Multiplies burn damage dealt per tick.
    /// Read in BaseEnemy.ApplyFire() before calling TakeDamage.
    /// </summary>
    public float FireDamageMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies how long the Burn debuff lasts.
    /// Read in BaseUnit.ApplyFire() when passing duration to ApplyDebuff.
    /// </summary>
    public float FireDurationMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the number of burn stacks applied per hit.
    /// Read in BaseUnit.ApplyFire() when passing stacks to ApplyDebuff.
    /// </summary>
    public float FireStacksMult { get; set; } = 1f;

    // --- POISON --------------------------------------------------------------

    /// <summary>
    /// Multiplies poison tick damage.
    /// TODO: Read in BaseUnit.ApplyPoison() / BaseEnemy.ApplyPoison() once implemented.
    /// </summary>
    public float PoisonDamageMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies poison debuff duration.
    /// TODO: Hook when poison is implemented.
    /// </summary>
    public float PoisonDurationMult { get; set; } = 1f;

    // --- SLOW ----------------------------------------------------------------

    /// <summary>
    /// Multiplies the slow percentage applied to enemies.
    /// Read in BaseEnemy.ApplySlow() before clamping the percent value.
    /// Applied here (enemy side) so ALL slow sources (direct calls, Wizard ability,
    /// debuff callbacks) are affected without double-applying.
    /// </summary>
    public float SlowStrengthMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the duration of Slow debuffs.
    /// Read in BaseUnit.ApplySlow() when passing duration to ApplyDebuff.
    /// </summary>
    public float SlowDurationMult { get; set; } = 1f;

    // --- STUN ----------------------------------------------------------------

    /// <summary>
    /// Multiplies stun duration on enemies.
    /// Read in BaseEnemy.ApplyStun() before applying the duration.
    /// </summary>
    public float StunDurationMult { get; set; } = 1f;

    // --- DAMAGE AMP ----------------------------------------------------------

    /// <summary>
    /// Multiplies DamageAmp stacks applied per hit.
    /// Read in BaseUnit.ApplyAmp() when passing stacks to ApplyDebuff.
    /// </summary>
    public float DamageAmpStrengthMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies how long DamageAmp debuffs last.
    /// Read in BaseUnit.ApplyAmp() when passing duration to ApplyDebuff.
    /// </summary>
    public float DamageAmpDurationMult { get; set; } = 1f;

    /// <summary>
    /// Global multiplier for chance-based on-hit effects (e.g. LuckyShot, WildFire).
    /// TODO: Hook into augment on-hit callbacks that use Random.value chance checks.
    /// </summary>
    public float OnHitEffectChanceMult { get; set; } = 1f;

    /// <summary>
    /// Scales how frequently all debuffs tick (base: every 1 second).
    /// Values below 1.0 make debuffs tick faster; above 1.0 slows them down.
    /// TODO: DEBUFF_TICK_RATE is currently a const in BaseEnemy. Convert it to a
    /// field that reads this multiplier when per-round tick-rate control is needed.
    /// </summary>
    public float DebuffTickRateMult { get; set; } = 1f;

    // =========================================================================
    // ECONOMY
    // =========================================================================

    /// <summary>
    /// If true, no interest is earned at the start of waves during this round.
    /// Read in CurrencyManager.newRound() — gates the GetInterest() call.
    /// </summary>
    public bool NoInterest { get; set; } = false;

    /// <summary>
    /// Multiplies interest earned this round. Ignored when NoInterest is true.
    /// Read in CurrencyManager.GetInterest() before adding the amount.
    /// </summary>
    public float InterestMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the base round income (default: 8 gold per wave).
    /// Read in CurrencyManager.newRound() when adding income.
    /// </summary>
    public float IncomeMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the gold cost of buying a tower from the shop.
    /// TODO: Hook into ShopManager's purchase cost calculation.
    /// </summary>
    public float TowerShopCostMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the cost of rerolling the shop.
    /// TODO: Hook into ShopManager reroll cost logic.
    /// </summary>
    public float RerollCostMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the gold value received when selling a tower.
    /// TODO: Hook into sell/refund logic in BoardManager or ShopManager.
    /// </summary>
    public float SellValueMult { get; set; } = 1f;

    // =========================================================================
    // WAVE / SPAWN MODIFIERS
    // =========================================================================

    /// <summary>
    /// Multiplies the number of enemies that spawn per wave.
    /// Read in WaveManager.RunSingleWave() as a local variable (does not permanently
    /// change WaveManager.enemiesPerWave — resets each wave with the context).
    /// </summary>
    public float EnemyCountMult { get; set; } = 1f;

    /// <summary>
    /// Multiplies the random delay between enemy spawns within a wave.
    /// Values below 1.0 make enemies swarm in faster; above 1.0 spreads them out.
    /// Read in WaveManager.RunSingleWave() when calculating spawn delay.
    /// </summary>
    public float EnemySpawnIntervalMult { get; set; } = 1f;

    // =========================================================================
    // RESET
    // =========================================================================

    /// <summary>
    /// Returns every field to its neutral default.
    /// Called automatically by RoundModifierManager.RemoveModifiers() after all
    /// modifier OnRoundEnd callbacks have fired, so modifiers can still read context
    /// during their own cleanup before the full reset happens.
    /// </summary>
    public void Reset()
    {
        // Tower stats
        TowerAttackDamageMult  = 1f;
        TowerAttackSpeedMult   = 1f;
        TowerAbilityPowerMult  = 1f;
        TowerRangeMult         = 1f;
        TowerAbilityCostMult   = 1f;
        TowerManaGainMult      = 1f;
        TowerMaxHealthMult     = 1f;
        ProjectileSpeedMult    = 1f;

        // Enemy stats
        EnemyHealthMult      = 1f;
        EnemySpeedMult       = 1f;
        EnemyDamageMult      = 1f;
        EnemyAttackSpeedMult = 1f;
        EnemyDamageAmpMult   = 1f;

        // Status effects
        FireDamageMult         = 1f;
        FireDurationMult       = 1f;
        FireStacksMult         = 1f;
        PoisonDamageMult       = 1f;
        PoisonDurationMult     = 1f;
        SlowStrengthMult       = 1f;
        SlowDurationMult       = 1f;
        StunDurationMult       = 1f;
        DamageAmpStrengthMult  = 1f;
        DamageAmpDurationMult  = 1f;
        OnHitEffectChanceMult  = 1f;
        DebuffTickRateMult     = 1f;

        // Economy
        NoInterest         = false;
        InterestMult       = 1f;
        IncomeMult         = 1f;
        TowerShopCostMult  = 1f;
        RerollCostMult     = 1f;
        SellValueMult      = 1f;

        // Wave / spawn
        EnemyCountMult         = 1f;
        EnemySpawnIntervalMult = 1f;
    }
}
