using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the pool of all round modifiers, drives weighted random selection each cycle,
/// and owns the apply/remove lifecycle that bookends each 3-wave round.
///
/// UNITY SETUP:
///   1. Add this component to the same GameObject as GameLoopManager.
///   2. Optionally assign BoardManager in the Inspector; otherwise it is found automatically.
///   3. RoundModifierSetup (on the same or any other GameObject) registers all modifiers
///      into this manager on Start.
///   4. GameLoopManager calls SelectModifiersForCycle() + ApplyModifiers() at the start
///      of each combat cycle, and RemoveModifiers() when the cycle ends.
///
/// ADDING MORE MODIFIERS PER ROUND:
///   Increase modifiersPerCycle in the Inspector. Mutual exclusion rules are respected
///   automatically during selection so incompatible modifiers won't stack.
/// </summary>
public class RoundModifierManager : MonoBehaviour
{
    public static RoundModifierManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Number of modifiers randomly chosen per 3-wave cycle. Default 1.")]
    [SerializeField] private int modifiersPerCycle = 1;

    [Header("References")]
    [Tooltip("BoardManager used by ForEachTower(). Found automatically if left empty.")]
    [SerializeField] private BoardManager boardManager;

    // Full pool of all registered modifiers (populated by RoundModifierSetup)
    private readonly List<RoundModifier> modifierPool = new List<RoundModifier>();

    // Modifiers selected and currently active for the ongoing cycle
    private readonly List<RoundModifier> activeModifiers = new List<RoundModifier>();

    /// <summary>
    /// Fired after modifiers are selected and applied at the start of each cycle.
    /// Subscribe to this in any UI component that needs to display the active modifiers.
    /// The list passed is the same reference as GetActiveModifiers() — read only, do not modify.
    /// </summary>
    public static event Action<IReadOnlyList<RoundModifier>> OnModifiersApplied;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (boardManager == null)
            boardManager = FindFirstObjectByType<BoardManager>();
    }

    // =========================================================================
    // REGISTRATION
    // Called once at game start by RoundModifierSetup.
    // =========================================================================

    /// <summary>
    /// Adds a modifier to the selection pool.
    /// Called by RoundModifierSetup during Start(). Safe to call at any time.
    /// </summary>
    public void RegisterModifier(RoundModifier modifier)
    {
        if (modifier != null)
            modifierPool.Add(modifier);
    }

    // =========================================================================
    // SELECTION
    // =========================================================================

    /// <summary>
    /// Randomly selects modifiers for the upcoming cycle using weighted random draw
    /// without replacement. Mutual exclusion is respected — once a modifier is chosen,
    /// all modifiers in the same category (or those it explicitly excludes) are removed
    /// from the draw pool for this cycle.
    ///
    /// Called by GameLoopManager just before the first wave of each new cycle.
    /// </summary>
    public void SelectModifiersForCycle()
    {
        activeModifiers.Clear();

        // Work from a temporary copy so the permanent pool is never modified
        List<RoundModifier> available = new List<RoundModifier>(modifierPool);

        for (int i = 0; i < modifiersPerCycle && available.Count > 0; i++)
        {
            RoundModifier chosen = WeightedRandomPick(available);
            if (chosen == null) break;

            activeModifiers.Add(chosen);

            // Remove the chosen modifier and any mutually exclusive ones from this draw
            available.RemoveAll(m => m == chosen || chosen.IsMutuallyExclusiveWith(m));
        }

        if (activeModifiers.Count > 0)
        {
            string names = string.Join(", ", activeModifiers.ConvertAll(m => m.Name));
            Debug.Log($"[RoundModifierManager] Selected for this cycle: {names}");
        }
        else
        {
            Debug.Log("[RoundModifierManager] No modifiers selected for this cycle.");
        }
    }

    /// <summary>Returns the currently active modifier list (read-only for UI use).</summary>
    public IReadOnlyList<RoundModifier> GetActiveModifiers() => activeModifiers;

    // =========================================================================
    // LIFECYCLE
    // =========================================================================

    /// <summary>
    /// Fires OnRoundStart on every active modifier.
    /// Called by GameLoopManager immediately before the first wave of a cycle.
    /// Modifiers set RoundModifierContext fields and apply direct tower changes here.
    /// </summary>
    public void ApplyModifiers()
    {
        for (int i = 0; i < activeModifiers.Count; i++)
        {
            activeModifiers[i].OnRoundStart?.Invoke();
            Debug.Log($"[RoundModifierManager] Applied: {activeModifiers[i].Name}");
        }

        // Notify any subscribed UI components so they can display the active modifiers
        OnModifiersApplied?.Invoke(activeModifiers);
    }

    /// <summary>
    /// Fires OnRoundEnd on every active modifier, then resets RoundModifierContext.
    /// Called by GameLoopManager after all waves in a cycle clear, before augment selection.
    ///
    /// ORDER: OnRoundEnd callbacks fire first (so direct tower changes can be reversed),
    ///        then Context.Reset() wipes all context multipliers clean.
    /// </summary>
    public void RemoveModifiers()
    {
        for (int i = 0; i < activeModifiers.Count; i++)
        {
            activeModifiers[i].OnRoundEnd?.Invoke();
            Debug.Log($"[RoundModifierManager] Removed: {activeModifiers[i].Name}");
        }

        // Reset context AFTER all cleanup so modifiers can still read it during OnRoundEnd
        if (RoundModifierContext.Instance != null)
            RoundModifierContext.Instance.Reset();
    }

    /// <summary>
    /// Notifies all active modifiers that a new tower was placed mid-round.
    /// Direct modifiers (e.g. HealthAndFireBuff) implement OnTowerPlaced to extend
    /// their effect to the new tower.
    ///
    /// UNITY HOOKUP: Call this method from wherever towers are placed onto the board.
    /// The best location is in BoardManager, UnitSpawner, or your drag-and-drop handler,
    /// right after the tower is initialized. Example:
    ///   RoundModifierManager.Instance?.NotifyTowerPlaced(newUnit);
    /// </summary>
    public void NotifyTowerPlaced(BaseUnit tower)
    {
        if (tower == null) return;
        for (int i = 0; i < activeModifiers.Count; i++)
            activeModifiers[i].OnTowerPlaced?.Invoke(tower);
    }

    // =========================================================================
    // UTILITY
    // =========================================================================

    /// <summary>
    /// Iterates every occupied cell in the board grid and invokes action on each
    /// living, non-null unit. Used by direct modifiers to apply or reverse stat
    /// changes across the whole board in one call.
    ///
    /// Example usage inside a modifier's OnRoundStart:
    ///   RoundModifierManager.Instance.ForEachTower(unit => unit.myData.MaxHP *= 1.25f);
    /// </summary>
    public void ForEachTower(Action<BaseUnit> action)
    {
        if (boardManager == null || boardManager.unitGrid == null) return;

        int width  = boardManager.unitGrid.GetLength(0);
        int height = boardManager.unitGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BaseUnit unit = boardManager.unitGrid[x, y];
                if (unit != null && !unit.IsDead)
                    action(unit);
            }
        }
    }

    // =========================================================================
    // DEBUG / TESTING  (safe to call in-editor; does nothing in a build)
    // =========================================================================

    /// <summary>
    /// Forces a specific modifier to be the only active modifier and immediately applies it.
    /// Used by RoundModifierDebugger. Searches by exact Name string (case-sensitive).
    /// Clears any previously active modifiers first without calling their OnRoundEnd,
    /// so call RemoveModifiers() before this if you need clean cleanup.
    /// </summary>
    public void ForceModifierByName(string name)
    {
        activeModifiers.Clear();

        RoundModifier found = modifierPool.Find(m => m.Name == name);
        if (found == null)
        {
            Debug.LogWarning($"[RoundModifierManager] ForceModifierByName: '{name}' not found in pool. " +
                             "Check that RoundModifierSetup has run and the name matches exactly.");
            return;
        }

        activeModifiers.Add(found);
        found.OnRoundStart?.Invoke();
        Debug.Log($"[RoundModifierManager] Force-applied: {found.Name}");
    }

    // =========================================================================
    // PRIVATE
    // =========================================================================

    /// <summary>
    /// Picks one modifier from the pool using weighted random selection.
    /// Modifiers with higher SelectionWeight are proportionally more likely to appear.
    /// </summary>
    private RoundModifier WeightedRandomPick(List<RoundModifier> pool)
    {
        float totalWeight = 0f;
        for (int i = 0; i < pool.Count; i++)
            totalWeight += Mathf.Max(0f, pool[i].SelectionWeight);

        if (totalWeight <= 0f) return null;

        float roll       = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < pool.Count; i++)
        {
            cumulative += Mathf.Max(0f, pool[i].SelectionWeight);
            if (roll <= cumulative)
                return pool[i];
        }

        // Fallback (floating point edge case)
        return pool[pool.Count - 1];
    }
}
