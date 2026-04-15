using System;
using UnityEngine;

/// <summary>
/// Data class representing a single round modifier.
/// Mirrors the structure of Augment.cs but scoped to one 3-wave cycle:
///   OnRoundStart  → fires when the cycle begins (apply effects)
///   OnRoundEnd    → fires after all 3 waves clear (reverse effects)
///   OnTowerPlaced → optional, fires when a new tower is placed mid-round
///
/// CREATING A NEW MODIFIER:
///   1. Create a new script in Assets/Scripts/RoundModifiers/ (copy an existing one).
///   2. Implement a CreateModifier() method that returns a configured RoundModifier.
///   3. Set Name, Description, Category, OnRoundStart, OnRoundEnd.
///   4. For modifiers that directly change tower stats (not just context fields),
///      also implement OnTowerPlaced so mid-round placements get the effect too.
///   5. Register it in RoundModifierSetup.cs.
///
/// CONTEXT-ONLY vs DIRECT modifiers:
///   Context-only  — just sets a field on RoundModifierContext (e.g. EnemySpeedMult=2).
///                   No cleanup needed in OnRoundEnd; Context.Reset() handles it.
///                   OnTowerPlaced is not needed since context is read at use-time.
///   Direct        — modifies tower myData fields directly (e.g. MaxHP).
///                   Must reverse changes in OnRoundEnd and implement OnTowerPlaced.
/// </summary>
public class RoundModifier
{
    /// <summary>Display name shown in the round modifier UI panel.</summary>
    public string Name;

    /// <summary>Flavor/mechanical description shown in the UI panel.</summary>
    public string Description;

    /// <summary>
    /// Icon displayed in the modifier UI notification.
    /// Assign via RoundModifierSetup after loading from Resources or a direct reference.
    /// </summary>
    public Sprite Icon;

    /// <summary>
    /// Category used for UI grouping, selection weighting, and mutual exclusion.
    /// Two modifiers in the same category are mutually exclusive by default —
    /// override IsMutuallyExclusiveWith() for finer control.
    /// </summary>
    public RoundModifierCategory Category;

    /// <summary>
    /// Weight used during weighted random selection in RoundModifierManager.
    /// Default 1.0. Set higher to make a modifier appear more frequently,
    /// lower (e.g. 0.5) to make it rarer. Set to 0 to temporarily disable.
    /// </summary>
    public float SelectionWeight = 1f;

    /// <summary>
    /// Called at the start of the round cycle, before the first wave.
    /// Set RoundModifierContext fields and apply any direct tower changes here.
    /// </summary>
    public Action OnRoundStart;

    /// <summary>
    /// Called at the end of the round cycle, after the last wave clears,
    /// before augment selection. Reverse any direct tower changes here.
    /// Context.Reset() is called automatically by RoundModifierManager
    /// AFTER all OnRoundEnd callbacks fire, so it is safe to read context here.
    /// </summary>
    public Action OnRoundEnd;

    /// <summary>
    /// Optional. Called when a new tower is placed mid-round.
    /// Implement this for DIRECT modifiers (e.g. HealthAndFireBuff) so that
    /// towers placed after the round starts also receive the effect.
    /// Context-only modifiers do not need this — context is read at use-time.
    ///
    /// UNITY HOOKUP: Call RoundModifierManager.Instance.NotifyTowerPlaced(unit)
    /// from wherever your code places towers onto the board (e.g. BoardManager,
    /// UnitSpawner, or the drag-and-drop placement handler).
    /// </summary>
    public Action<BaseUnit> OnTowerPlaced;

    public RoundModifier(
        string name,
        string description,
        RoundModifierCategory category,
        Action onRoundStart,
        Action onRoundEnd,
        Action<BaseUnit> onTowerPlaced = null,
        float selectionWeight = 1f)
    {
        Name            = name;
        Description     = description;
        Category        = category;
        OnRoundStart    = onRoundStart;
        OnRoundEnd      = onRoundEnd;
        OnTowerPlaced   = onTowerPlaced;
        SelectionWeight = selectionWeight;
    }

    /// <summary>
    /// Returns true if this modifier cannot be active at the same time as the other.
    /// Default: two modifiers sharing the same Category are mutually exclusive.
    /// Override or replace this logic in RoundModifierManager if you need different rules.
    /// </summary>
    public virtual bool IsMutuallyExclusiveWith(RoundModifier other)
    {
        return other != null && other.Category == Category;
    }
}

/// <summary>
/// Categories for round modifiers. Used for UI grouping, selection weighting,
/// and mutual exclusion. Add new values here as modifier types expand.
/// </summary>
public enum RoundModifierCategory
{
    TowerDebuff,   // Penalizes player towers (attack speed, damage, ability cost…)
    TowerBuff,     // Buffs player towers (health, fire damage…)
    EnemyBuff,     // Makes enemies stronger (speed, health, damage…)
    EnemyDebuff,   // Makes enemies weaker — reserved for future modifiers
    EconomyDebuff, // Penalizes economy (no interest, higher shop costs…)
    EconomyBuff,   // Boosts economy — reserved for future modifiers
    WaveModifier,  // Changes wave structure (enemy count, spawn speed…)
    Mixed,         // Affects multiple categories (e.g. buff enemies but boost fire damage)
}
