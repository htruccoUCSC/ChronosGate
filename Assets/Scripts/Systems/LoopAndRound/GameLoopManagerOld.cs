using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Legacy guided loop:
/// Game Start -> Augment Selection -> 3x(Shop -> Wave) -> Back to Augment Selection.
/// </summary>
public class GameLoopManagerOld : MonoBehaviour
{
    public static GameLoopManagerOld Instance { get; private set; }

    [Header("References")]
    [SerializeField] protected AugmentSelectionUI augmentSelectionUI;
    [SerializeField] protected ShopManagerOld shopManager;
    [SerializeField] protected WaveManager waveManager;
    public AugmentManager augmentManager;

    public NewRound newRound;

    [Header("Settings")]
    [SerializeField] protected int wavesPerAugmentCycle = 3;

    [Header("Unit Unlocks")]
    [SerializeField] private bool useUnitUnlocks = true;
    [SerializeField] private GameObject unlockPickupPrefab;
    [SerializeField] private Transform pickupSpawnLocation;
    [SerializeField] private float unlockPhaseDelay = 1f;
    [SerializeField] private float unlockPickupTimeoutSeconds = 10f;

    protected int currentWaveInCycle = 0;
    protected bool isGameActive = false;
    private bool waitingForNextRound = false;
    private bool waitingForPickup = false;
    private UnlockPickup currentPickup;
    private UnitUnlockManager unlockManager;
    private DatabaseLoader databaseLoader;

    public enum GameState
    {
        AugmentSelection,
        Shopping,
        Combat,
        GameOver
    }

    public GameState CurrentState { get; private set; } = GameState.AugmentSelection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    protected void Start()
    {
        if (augmentSelectionUI == null)
        {
            augmentSelectionUI = FindFirstObjectByType<AugmentSelectionUI>();
        }

        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManagerOld>();
        }

        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }

        if (newRound == null)
        {
            newRound = FindFirstObjectByType<NewRound>();
        }

        unlockManager = UnitUnlockManager.Instance;
        databaseLoader = FindFirstObjectByType<DatabaseLoader>();

        if (FindFirstObjectByType<GameSpeedButton>() == null)
        {
            GameObject speedButtonObject = new GameObject("GameSpeedButtonController");
            speedButtonObject.AddComponent<GameSpeedButton>();
        }

        if (augmentSelectionUI != null)
        {
            augmentSelectionUI.OnAugmentSelected += OnAugmentSelected;
        }

        StartCoroutine(StartGameLoopDelayed());
    }

    protected virtual IEnumerator StartGameLoopDelayed()
    {
        yield return null;

        isGameActive = true;
        currentWaveInCycle = 0;
        ShowAugmentSelection();
    }

    protected void ShowAugmentSelection()
    {
        CurrentState = GameState.AugmentSelection;
        Debug.Log("=== AUGMENT SELECTION PHASE ===");

        if (augmentSelectionUI != null)
        {
            augmentSelectionUI.ShowAugmentSelection();
        }
    }

    private void OnAugmentSelected(int augmentIndex)
    {
        Debug.Log($"Player selected augment {augmentIndex}. Starting shop phase.");
        currentWaveInCycle = 0;
        StartShoppingPhase();
    }

    protected void StartShoppingPhase()
    {
        CurrentState = GameState.Shopping;
        Debug.Log($"=== SHOPPING PHASE (Wave {currentWaveInCycle + 1}/{wavesPerAugmentCycle}) ===");

        if (shopManager != null)
        {
            shopManager.OpenShop();
        }

        waitingForNextRound = true;
    }

    public void OnNextRoundPressed()
    {
        if (!waitingForNextRound)
        {
            return;
        }

        waitingForNextRound = false;

        Debug.Log("Next Round pressed. Starting combat phase...");
        augmentManager.ApplyAllActiveAugments();
        StartCombatPhase();
    }

    private void StartCombatPhase()
    {
        CurrentState = GameState.Combat;
        Debug.Log($"=== COMBAT PHASE - Wave {waveManager.currentWave} (Cycle: {currentWaveInCycle + 1}/{wavesPerAugmentCycle}) ===");

        if (GameSpeedButton.Instance != null)
        {
            GameSpeedButton.Instance.SetPaused(false);
            GameSpeedButton.Instance.ResetToDefaultSpeed();
        }

        if (waveManager != null)
        {
            waveManager.StartNextWave();
        }

        StartCoroutine(WaitForWaveCompletion());
    }

    protected virtual IEnumerator WaitForWaveCompletion()
    {
        yield return new WaitUntil(() => waveManager.IsWaveComplete() || waveManager.IsGameOver());

        if (waveManager.IsGameOver() || waveManager.lives <= 0)
        {
            GameOver();
            yield break;
        }

        Debug.Log("Wave cleared!");

        if (newRound != null)
        {
            newRound.startNewRound();
        }

        WaveUnlock pendingUnlock = CheckForUnlock(waveManager.currentWave);

        currentWaveInCycle++;
        Debug.Log($"Waves completed in cycle: {currentWaveInCycle}/{wavesPerAugmentCycle}");

        if (pendingUnlock != null)
        {
            yield return StartCoroutine(HandleUnlockPhase(pendingUnlock));
        }

        if (currentWaveInCycle >= wavesPerAugmentCycle)
        {
            Debug.Log("Cycle complete! Returning to augment selection...");
            yield return new WaitForSeconds(1f);
            ShowAugmentSelection();
        }
        else
        {
            Debug.Log($"Moving to next shopping phase... (Next: Wave {currentWaveInCycle + 1}/{wavesPerAugmentCycle})");
            yield return new WaitForSeconds(1f);
            waveManager.expandBoard();
            StartShoppingPhase();
        }
    }

    protected void GameOver()
    {
        CurrentState = GameState.GameOver;
        isGameActive = false;
        Debug.Log("=== GAME OVER ===");

        Invoke(nameof(LoadGameOverScene), 3f);
    }

    private void LoadGameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    public bool IsGameActive()
    {
        return isGameActive;
    }

    private WaveUnlock CheckForUnlock(int waveNumber)
    {
        if (!useUnitUnlocks || unlockManager == null || !unlockManager.IsReady())
        {
            return null;
        }

        return unlockManager.UpdateWaveProgress(waveNumber);
    }

    private IEnumerator HandleUnlockPhase(WaveUnlock unlock)
    {
        if (unlockPickupPrefab == null)
        {
            Debug.LogWarning("[GameLoopManagerOld] Unlock pickup prefab is not assigned. Skipping unlock phase.");
            yield break;
        }

        UnitDefinition unitDef = GetUnitDefinition(unlock.unitID);
        if (unitDef == null)
        {
            Debug.LogWarning($"[GameLoopManagerOld] Could not find UnitDefinition for unlock {unlock.unitID}.");
            yield break;
        }

        yield return new WaitForSeconds(unlockPhaseDelay);

        currentPickup = SpawnUnlockPickup(unitDef, unlock);
        if (currentPickup == null)
        {
            yield break;
        }

        waitingForPickup = true;
        currentPickup.OnPickupClaimed += OnPickupClaimed;
        float timeoutAt = Time.time + Mathf.Max(0.5f, unlockPickupTimeoutSeconds);

        Debug.Log($"[GameLoopManagerOld] Waiting for unlock pickup claim: unit={unitDef.Name}, timeout={unlockPickupTimeoutSeconds:0.##}s");

        yield return new WaitUntil(() => !waitingForPickup || Time.time >= timeoutAt);

        if (waitingForPickup)
        {
            Debug.LogWarning($"[GameLoopManagerOld] Unlock pickup timed out for unit {unitDef.Name}. Continuing loop.");
            if (currentPickup != null)
            {
                currentPickup.ClaimPickup();
            }
            else if (unlockManager != null)
            {
                unlockManager.UnlockUnit(unlock.unitID);
                OnPickupClaimed(unitDef);
            }
        }
    }

    private UnlockPickup SpawnUnlockPickup(UnitDefinition unitDef, WaveUnlock unlock)
    {
        Vector3 spawnPosition = pickupSpawnLocation != null ? pickupSpawnLocation.position : Vector3.zero;
        Debug.Log($"[GameLoopManagerOld] Spawning unlock pickup for {unitDef.Name} at world position {spawnPosition}.");
        GameObject pickupObject = Instantiate(unlockPickupPrefab, spawnPosition, Quaternion.identity);
        UnlockPickup pickup = pickupObject.GetComponent<UnlockPickup>();

        if (pickup == null)
        {
            Debug.LogWarning("[GameLoopManagerOld] Unlock pickup prefab is missing UnlockPickup.");
            Destroy(pickupObject);
            return null;
        }

        pickup.Initialize(unitDef, unlock);
        return pickup;
    }

    private UnitDefinition GetUnitDefinition(string unitID)
    {
        if (databaseLoader == null || databaseLoader.UnitLookup == null)
        {
            return null;
        }

        databaseLoader.UnitLookup.TryGetValue(unitID, out UnitDefinition unitDef);
        return unitDef;
    }

    private void OnPickupClaimed(UnitDefinition unitDef)
    {
        Debug.Log($"[GameLoopManagerOld] Unlock pickup claimed/cleared for {unitDef?.Name ?? "unknown unit"}.");
        waitingForPickup = false;

        if (currentPickup != null)
        {
            currentPickup.OnPickupClaimed -= OnPickupClaimed;
            currentPickup = null;
        }
    }

    private void OnDestroy()
    {
        if (augmentSelectionUI != null)
        {
            augmentSelectionUI.OnAugmentSelected -= OnAugmentSelected;
        }

        if (currentPickup != null)
        {
            currentPickup.OnPickupClaimed -= OnPickupClaimed;
        }
    }
}
