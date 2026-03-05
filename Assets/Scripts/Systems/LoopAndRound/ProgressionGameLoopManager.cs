using UnityEngine;
using System.Collections;

/// <summary>
/// Extended game loop manager for progression mode.
/// Checks for unit unlocks after wave completion and spawns pickup objects.
/// </summary>
public class ProgressionGameLoopManager : GameLoopManager
{
    [Header("Progression Settings")]
    [SerializeField] private GameObject unlockPickupPrefab;
    [SerializeField] private Transform pickupSpawnLocation;
    [SerializeField] private float unlockPhaseDelay = 1f;
    [SerializeField] private bool skipInitialAugmentSelection = true;
    
    private UnitUnlockManager unlockManager;
    private DatabaseLoader databaseLoader;
    private LaneProgressionManager laneProgressionManager;
    private bool waitingForPickup = false;
    private UnlockPickup currentPickup;
    public WaveManager board;
    
    private new void Start()
    {
        // Initialize progression-specific components FIRST
        unlockManager = UnitUnlockManager.Instance;
        if (unlockManager == null)
        {
            Debug.LogError("[ProgressionGameLoopManager] UnitUnlockManager not found!");
        }
        
        databaseLoader = FindFirstObjectByType<DatabaseLoader>();
        if (databaseLoader == null)
        {
            Debug.LogError("[ProgressionGameLoopManager] DatabaseLoader not found!");
        }
        
        laneProgressionManager = LaneProgressionManager.Instance;
        if (laneProgressionManager == null)
        {
            Debug.LogWarning("[ProgressionGameLoopManager] LaneProgressionManager not found! Lane progression disabled.");
        }
        
        // CRITICAL: Call base Start() to initialize the game loop!
        base.Start();
    }
    
    /// <summary>
    /// Override to optionally skip initial augment selection in progression mode.
    /// </summary>
    protected override IEnumerator StartGameLoopDelayed()
    {
        // Wait one frame to ensure all Start() methods have run
        yield return null;

        // Check for duplicate or missing components (same as base)
        var allAugmentUIs = FindObjectsByType<AugmentSelectionUI>(FindObjectsSortMode.None);
        Debug.Log($"[DIAGNOSTIC] Found {allAugmentUIs.Length} AugmentSelectionUI components in scene");
        foreach (var ui in allAugmentUIs)
        {
            Debug.Log($"[DIAGNOSTIC] AugmentSelectionUI on GameObject: {ui.gameObject.name}");
        }
        
        var allShopManagers = FindObjectsByType<ShopManager>(FindObjectsSortMode.None);
        Debug.Log($"[DIAGNOSTIC] Found {allShopManagers.Length} ShopManager components in scene");
        
        var allWaveManagers = FindObjectsByType<WaveManager>(FindObjectsSortMode.None);
        Debug.Log($"[DIAGNOSTIC] Found {allWaveManagers.Length} WaveManager components in scene");
        
        isGameActive = true;
        currentWaveInCycle = 0;
        
        // In progression mode, optionally skip initial augment selection and go straight to shop
        if (skipInitialAugmentSelection)
        {
            Debug.Log("[ProgressionGameLoopManager] Skipping initial augment selection, starting with shop phase.");
            StartShoppingPhase();
        }
        else
        {
            ShowAugmentSelection();
        }
    }
    
    /// <summary>
    /// Override to add unlock check phase.
    /// </summary>
    protected override IEnumerator WaitForWaveCompletion()
    {
        float originalSpawnDelay = waveManager != null ? waveManager.spawnDelay : 0f;
        float originalTimeBetweenWaves = waveManager != null ? waveManager.timeBetweenWaves : 0f;

        if (waveManager != null)
        {
            float speedMult = Mathf.Max(1f, roundSpeedMultiplier);
            waveManager.spawnDelay = Mathf.Max(0.05f, waveManager.spawnDelay / speedMult);
            waveManager.timeBetweenWaves = Mathf.Max(0f, waveManager.timeBetweenWaves / speedMult);
        }

        int wavesRemaining = Mathf.Max(1, wavesPerRound);

        while (wavesRemaining > 0)
        {
            yield return new WaitUntil(() => waveManager.IsWaveComplete() || waveManager.IsGameOver());

            if (waveManager.IsGameOver() || waveManager.lives <= 0)
            {
                if (waveManager != null)
                {
                    waveManager.spawnDelay = originalSpawnDelay;
                    waveManager.timeBetweenWaves = originalTimeBetweenWaves;
                }

                GameOver();
                yield break;
            }

            Debug.Log("Wave cleared!");

            int completedWaveNumber = waveManager.currentWave;

            bool lanesUnlocked = CheckForLaneUnlock(completedWaveNumber);
            if (lanesUnlocked)
            {
                yield return new WaitForSeconds(unlockPhaseDelay * 0.5f);
            }

            WaveUnlock pendingUnlock = CheckForUnlock(completedWaveNumber);

            currentWaveInCycle++;
            wavesRemaining--;
            Debug.Log($"Waves completed in cycle: {currentWaveInCycle}/{wavesPerAugmentCycle}");

            if (pendingUnlock != null)
            {
                yield return StartCoroutine(HandleUnlockPhase(pendingUnlock));
            }

            if (wavesRemaining > 0)
            {
                waveManager.StartNextWave();
            }
        }

        if (waveManager != null)
        {
            waveManager.spawnDelay = originalSpawnDelay;
            waveManager.timeBetweenWaves = originalTimeBetweenWaves;
        }

        if (currentWaveInCycle >= wavesPerAugmentCycle)
        {
            Debug.Log("Cycle complete! Returning to augment selection...");
            yield return new WaitForSeconds(0.35f);
            ShowAugmentSelection();
        }
        else
        {
            Debug.Log($"Moving to next shopping phase... (Next: Wave {currentWaveInCycle + 1}/{wavesPerAugmentCycle})");
            yield return new WaitForSeconds(0.35f);
            newRound.startNewRound();
            StartShoppingPhase();
        }
    }
    
    /// <summary>
    /// Checks if the current wave unlocks new lanes.
    /// </summary>
    private bool CheckForLaneUnlock(int waveNumber)
    {
        if (laneProgressionManager == null) return false;
        
        bool unlocked = laneProgressionManager.CheckAndUnlockLanes(waveNumber);
        
        if (unlocked)
        {
            Debug.Log($"[ProgressionGameLoopManager] Wave {waveNumber} unlocked new lanes!");
        }
        
        return unlocked;
    }
    
    /// <summary>
    /// Checks if the current wave unlocks a new unit.
    /// </summary>
    private WaveUnlock CheckForUnlock(int waveNumber)
    {
        if (unlockManager == null) return null;
        
        WaveUnlock unlock = unlockManager.UpdateWaveProgress(waveNumber);
        
        if (unlock != null)
        {
            Debug.Log($"[ProgressionGameLoopManager] Wave {waveNumber} unlocks: {unlock.unitID}");
        }
        
        return unlock;
    }
    
    /// <summary>
    /// Spawns the unlock pickup and waits for player to claim it.
    /// </summary>
    private IEnumerator HandleUnlockPhase(WaveUnlock unlock)
    {
        Debug.Log($"=== UNLOCK PHASE: {unlock.unitID} ===");
        
        yield return new WaitForSeconds(unlockPhaseDelay);
        
        // Get the unit definition
        UnitDefinition unitDef = GetUnitDefinition(unlock.unitID);
        if (unitDef == null)
        {
            Debug.LogError($"[ProgressionGameLoopManager] Could not find UnitDefinition for {unlock.unitID}");
            yield break;
        }
        
        // Spawn the pickup
        currentPickup = SpawnUnlockPickup(unitDef, unlock);
        if (currentPickup == null)
        {
            Debug.LogError("[ProgressionGameLoopManager] Failed to spawn unlock pickup!");
            yield break;
        }
        
        // Wait for player to claim
        waitingForPickup = true;
        currentPickup.OnPickupClaimed += OnPickupClaimed;
        
        yield return new WaitUntil(() => !waitingForPickup);
        
        Debug.Log("[ProgressionGameLoopManager] Unlock claimed, continuing...");
    }
    
    /// <summary>
    /// Spawns the unlock pickup GameObject.
    /// </summary>
    private UnlockPickup SpawnUnlockPickup(UnitDefinition unitDef, WaveUnlock unlock)
    {
        if (unlockPickupPrefab == null)
        {
            Debug.LogError("[ProgressionGameLoopManager] unlockPickupPrefab is not assigned!");
            return null;
        }
        
        Vector3 spawnPos = pickupSpawnLocation != null 
            ? pickupSpawnLocation.position 
            : new Vector3(0, 0, 0);
        
        GameObject pickupObj = Instantiate(unlockPickupPrefab, spawnPos, Quaternion.identity);
        UnlockPickup pickup = pickupObj.GetComponent<UnlockPickup>();
        
        if (pickup == null)
        {
            Debug.LogError("[ProgressionGameLoopManager] UnlockPickup component not found on prefab!");
            Destroy(pickupObj);
            return null;
        }
        
        pickup.Initialize(unitDef, unlock);
        return pickup;
    }
    
    /// <summary>
    /// Called when player claims the unlock pickup.
    /// </summary>
    private void OnPickupClaimed(UnitDefinition unitDef)
    {
        Debug.Log($"[ProgressionGameLoopManager] Player claimed unit: {unitDef.Name}");
        
        waitingForPickup = false;
        
        if (currentPickup != null)
        {
            currentPickup.OnPickupClaimed -= OnPickupClaimed;
            currentPickup = null;
        }
    }
    
    /// <summary>
    /// Gets a UnitDefinition from the database by ID.
    /// </summary>
    private UnitDefinition GetUnitDefinition(string unitID)
    {
        if (databaseLoader == null || databaseLoader.UnitLookup == null)
        {
            Debug.LogError("[ProgressionGameLoopManager] DatabaseLoader or UnitLookup is null!");
            return null;
        }
        
        if (databaseLoader.UnitLookup.TryGetValue(unitID, out UnitDefinition unitDef))
        {
            return unitDef;
        }
        
        Debug.LogWarning($"[ProgressionGameLoopManager] UnitDefinition not found for ID: {unitID}");
        return null;
    }
}

