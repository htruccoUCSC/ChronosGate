using UnityEngine;

/// <summary>
/// In-editor testing utility for the round modifier system.
/// Attach to any active GameObject in your scene while testing.
///
/// UNITY SETUP:
///   1. Add this component to any GameObject in the test scene.
///   2. Right-click the component header in the Inspector to see all test methods.
///   3. All methods can also be called from the Unity Editor toolbar via
///      right-click > [ContextMenu name].
///
/// REMOVE THIS COMPONENT before shipping — it is for testing only.
/// </summary>
public class RoundModifierDebugger : MonoBehaviour
{
    // =========================================================================
    // FORCE INDIVIDUAL MODIFIERS
    // Each method removes any currently active modifier, then forces exactly
    // one specific modifier to apply so you can verify its effect in isolation.
    // =========================================================================

    [ContextMenu("Test / Force Blitzkrieg (2x speed, half health)")]
    public void ForceBlitzkrieg()
    {
        ForceModifier("Blitzkrieg");
    }

    [ContextMenu("Test / Force Mana Tax (abilities cost more)")]
    public void ForceManaT()
    {
        ForceModifier("Mana Tax");
    }

    [ContextMenu("Test / Force Recession (no interest)")]
    public void ForceRecession()
    {
        ForceModifier("Recession");
    }

    [ContextMenu("Test / Force Bulwark Flame (tower HP + fire damage)")]
    public void ForceBulwarkFlame()
    {
        ForceModifier("Bulwark Flame");
    }

    [ContextMenu("Test / Force Sluggish Arsenal (10% attack speed reduction)")]
    public void ForceSluggishArsenal()
    {
        ForceModifier("Sluggish Arsenal");
    }

    // =========================================================================
    // LIFECYCLE CONTROLS
    // =========================================================================

    [ContextMenu("Test / Apply Current Modifiers (simulate cycle start)")]
    public void ApplyCurrentModifiers()
    {
        RoundModifierManager mgr = RoundModifierManager.Instance;
        if (mgr == null) { LogMissing("RoundModifierManager"); return; }
        mgr.ApplyModifiers();
        Debug.Log("[Debugger] ApplyModifiers() called.");
        PrintContextValues();
    }

    [ContextMenu("Test / Remove Modifiers and Reset Context (simulate cycle end)")]
    public void RemoveModifiersAndReset()
    {
        RoundModifierManager mgr = RoundModifierManager.Instance;
        if (mgr == null) { LogMissing("RoundModifierManager"); return; }
        mgr.RemoveModifiers();
        Debug.Log("[Debugger] RemoveModifiers() called. Context reset.");
        PrintContextValues();
    }

    [ContextMenu("Test / Run Full Select → Apply Cycle")]
    public void RunFullCycle()
    {
        RoundModifierManager mgr = RoundModifierManager.Instance;
        if (mgr == null) { LogMissing("RoundModifierManager"); return; }
        mgr.SelectModifiersForCycle();
        mgr.ApplyModifiers();
        Debug.Log("[Debugger] Full cycle: Select + Apply complete.");
        PrintContextValues();
    }

    // =========================================================================
    // INSPECTION
    // =========================================================================

    [ContextMenu("Test / Print Context Values (all multipliers)")]
    public void PrintContextValues()
    {
        RoundModifierContext ctx = RoundModifierContext.Instance;
        if (ctx == null) { LogMissing("RoundModifierContext"); return; }

        // Each section is a separate Debug.Log so every line appears in the Console
        // without needing to click to expand a collapsed message.
        Debug.Log("[Context] === TOWER ===");
        Debug.Log($"[Context]  TowerAttackDamageMult  = {ctx.TowerAttackDamageMult}");
        Debug.Log($"[Context]  TowerAttackSpeedMult   = {ctx.TowerAttackSpeedMult}");
        Debug.Log($"[Context]  TowerAbilityPowerMult  = {ctx.TowerAbilityPowerMult}");
        Debug.Log($"[Context]  TowerRangeMult         = {ctx.TowerRangeMult}");
        Debug.Log($"[Context]  TowerAbilityCostMult   = {ctx.TowerAbilityCostMult}");
        Debug.Log($"[Context]  TowerManaGainMult      = {ctx.TowerManaGainMult}");
        Debug.Log($"[Context]  TowerMaxHealthMult     = {ctx.TowerMaxHealthMult}");
        Debug.Log($"[Context]  ProjectileSpeedMult    = {ctx.ProjectileSpeedMult}");

        Debug.Log("[Context] === ENEMY ===");
        Debug.Log($"[Context]  EnemyHealthMult        = {ctx.EnemyHealthMult}");
        Debug.Log($"[Context]  EnemySpeedMult         = {ctx.EnemySpeedMult}");
        Debug.Log($"[Context]  EnemyDamageMult        = {ctx.EnemyDamageMult}");
        Debug.Log($"[Context]  EnemyAttackSpeedMult   = {ctx.EnemyAttackSpeedMult}");
        Debug.Log($"[Context]  EnemyDamageAmpMult     = {ctx.EnemyDamageAmpMult}");

        Debug.Log("[Context] === STATUS EFFECTS ===");
        Debug.Log($"[Context]  FireDamageMult         = {ctx.FireDamageMult}");
        Debug.Log($"[Context]  FireDurationMult       = {ctx.FireDurationMult}");
        Debug.Log($"[Context]  FireStacksMult         = {ctx.FireStacksMult}");
        Debug.Log($"[Context]  SlowStrengthMult       = {ctx.SlowStrengthMult}");
        Debug.Log($"[Context]  SlowDurationMult       = {ctx.SlowDurationMult}");
        Debug.Log($"[Context]  StunDurationMult       = {ctx.StunDurationMult}");
        Debug.Log($"[Context]  DamageAmpStrengthMult  = {ctx.DamageAmpStrengthMult}");
        Debug.Log($"[Context]  DamageAmpDurationMult  = {ctx.DamageAmpDurationMult}");

        Debug.Log("[Context] === ECONOMY ===");
        Debug.Log($"[Context]  NoInterest             = {ctx.NoInterest}");
        Debug.Log($"[Context]  InterestMult           = {ctx.InterestMult}");
        Debug.Log($"[Context]  IncomeMult             = {ctx.IncomeMult}");
        Debug.Log($"[Context]  TowerShopCostMult      = {ctx.TowerShopCostMult}");
        Debug.Log($"[Context]  RerollCostMult         = {ctx.RerollCostMult}");
        Debug.Log($"[Context]  SellValueMult          = {ctx.SellValueMult}");

        Debug.Log("[Context] === WAVE ===");
        Debug.Log($"[Context]  EnemyCountMult         = {ctx.EnemyCountMult}");
        Debug.Log($"[Context]  EnemySpawnIntervalMult = {ctx.EnemySpawnIntervalMult}");
    }

    [ContextMenu("Test / Print Active Modifiers")]
    public void PrintActiveModifiers()
    {
        RoundModifierManager mgr = RoundModifierManager.Instance;
        if (mgr == null) { LogMissing("RoundModifierManager"); return; }

        var active = mgr.GetActiveModifiers();
        if (active.Count == 0)
        {
            Debug.Log("[Debugger] No modifiers currently active.");
            return;
        }

        for (int i = 0; i < active.Count; i++)
            Debug.Log($"[Debugger] Active modifier [{i}]: {active[i].Name} — {active[i].Description}");
    }

    [ContextMenu("Test / Print All Tower Stats on Board")]
    public void PrintAllTowerStats()
    {
        RoundModifierManager mgr = RoundModifierManager.Instance;
        if (mgr == null) { LogMissing("RoundModifierManager"); return; }

        mgr.ForEachTower(unit =>
        {
            if (unit.myData == null) return;
            Debug.Log(
                $"[Stats] Tower '{unit.name}' | " +
                $"HP {unit.myData.CurrentHP:F1}/{unit.myData.MaxHP:F1} | " +
                $"DMG {unit.myData.GetModifiedDamage():F2} | " +
                $"SPD {unit.myData.GetModifiedAttackSpeed():F2} | " +
                $"AP {unit.myData.GetModifiedAbilityPower():F2} | " +
                $"RNG {unit.myData.GetModifiedRange():F2}"
            );
        });
    }

    [ContextMenu("Test / Print Mana Stats for All Towers (verify Mana Tax)")]
    public void PrintManaTowerStats()
    {
        RoundModifierManager mgr = RoundModifierManager.Instance;
        if (mgr == null) { LogMissing("RoundModifierManager"); return; }

        RoundModifierContext ctx = RoundModifierContext.Instance;
        float costMult = ctx != null ? ctx.TowerAbilityCostMult : 1f;

        Debug.Log($"[Mana] TowerAbilityCostMult = {costMult}  (1.0 = normal, 1.3 = Mana Tax active)");

        mgr.ForEachTower(unit =>
        {
            if (unit.myData == null || unit.myData.BaseDef == null) return;

            float baseCost      = unit.myData.BaseDef.AbilityManaCost;
            float effectiveCost = baseCost * costMult;

            Debug.Log(
                $"[Mana] Tower '{unit.name}' | " +
                $"Mana {unit.myData.CurrentMana:F1} / {effectiveCost:F1} (effective cost) | " +
                $"Base cost {baseCost:F1} | " +
                $"Mana/shot 10 x {(ctx != null ? ctx.TowerManaGainMult : 1f)} = {10f * (ctx != null ? ctx.TowerManaGainMult : 1f):F1}"
            );
        });
    }

    // =========================================================================
    // PRIVATE HELPERS
    // =========================================================================

    /// <summary>
    /// Removes any active modifiers, finds the modifier with the given name in the pool,
    /// forces it as the only active modifier, then applies it.
    /// </summary>
    private void ForceModifier(string modifierName)
    {
        RoundModifierManager mgr = RoundModifierManager.Instance;
        if (mgr == null) { LogMissing("RoundModifierManager"); return; }

        // Clean up any previously active modifier first
        mgr.RemoveModifiers();

        // Use reflection-free access: temporarily set all other weights to 0
        // by iterating the pool and picking by name, then calling the internal
        // apply path directly. Since the pool is private we go through the public API:
        // force-select by running SelectModifiersForCycle until we get what we want,
        // OR — simpler — just grab all actives after a targeted select.
        // The cleanest approach without modifying the manager is to expose a
        // ForceModifier method. For now we use the debug-only path below.

        // NOTE: This method requires RoundModifierManager to expose ForceModifierByName.
        // That method is defined at the bottom of RoundModifierManager via the
        // #if UNITY_EDITOR block added by this debugger's presence.
        // If you see a compile error here, add the method manually (see below).
        mgr.ForceModifierByName(modifierName);

        Debug.Log($"[Debugger] Forced modifier: '{modifierName}'");
        PrintContextValues();
    }

    private void LogMissing(string componentName)
    {
        Debug.LogError($"[RoundModifierDebugger] {componentName} not found in scene. " +
                       $"Make sure it exists on a GameObject and has initialized.");
    }
}
