using UnityEngine;

/// <summary>
/// Creates all RoundModifier instances and registers them with RoundModifierManager.
/// This is the single file to edit when adding new modifiers to the game.
/// Mirrors the role of AugmentSetup.cs in the augment system.
///
/// UNITY SETUP:
///   Add this component to any GameObject in the main game scene.
///   It runs on Start() after RoundModifierManager has already initialized.
///   No Inspector wiring required.
///
/// ADDING A NEW MODIFIER:
///   1. Create a new script in Assets/Scripts/RoundModifiers/ using an existing one as a template.
///   2. Implement a CreateModifier() method that returns a configured RoundModifier.
///   3. Add one line below: manager.RegisterModifier(new YourModifier().CreateModifier());
/// </summary>
public class RoundModifierSetup : MonoBehaviour
{
    private void Start()
    {
        RoundModifierManager manager = RoundModifierManager.Instance;
        if (manager == null)
        {
            Debug.LogError("[RoundModifierSetup] RoundModifierManager not found in scene. " +
                           "Ensure a GameObject with RoundModifierManager exists and its Awake() " +
                           "runs before this Start().");
            return;
        }

        // ── Register modifiers here ──────────────────────────────────────────
        // Each line creates one modifier and adds it to the selection pool.
        // Order does not matter — selection is random and weighted.

        manager.RegisterModifier(new EnemiesDoubleSpeedHalfHealth().CreateModifier());
        manager.RegisterModifier(new AbilitiesCostMore().CreateModifier());
        manager.RegisterModifier(new NoInterestModifier().CreateModifier());
        manager.RegisterModifier(new HealthAndFireBuff().CreateModifier());
        manager.RegisterModifier(new ReducedTowerAttackSpeed().CreateModifier());

        Debug.Log("[RoundModifierSetup] All round modifiers registered.");
    }
}
