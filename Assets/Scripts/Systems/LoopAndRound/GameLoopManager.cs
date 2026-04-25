using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game loop manager that orchestrates the flow:
/// Game Start -> 3x(Combat) -> Augment Selection -> repeat
/// </summary>
public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] protected AugmentSelectionUI augmentSelectionUI;
    [SerializeField] protected ShopManager shopManager;
    [SerializeField] protected WaveManager waveManager;
    [SerializeField] protected MusicController musicController;
    [SerializeField] protected EnemyPreviewManager enemyPreviewManager;
    public AugmentManager augmentManager;

    // Round modifier manager — found automatically if not assigned in the Inspector.
    // Drives SelectModifiersForCycle() + ApplyModifiers() at the start of each cycle
    // and RemoveModifiers() at the end, before augment selection.
    protected RoundModifierManager modifierManager;

    public NewRound newRound;


    [Header("Settings")]
    [SerializeField] protected int wavesPerAugmentCycle = 3;
    [SerializeField] protected bool autoStartRounds = true;
    [SerializeField] protected float autoStartRoundDelay = 1.5f;
    [SerializeField] protected bool reopenShopEachRound = true;

    protected int currentWaveInCycle = 0;
    protected bool isGameActive = false;
    public TileMapManager tileMapManager;
    public int roundsOfGrowth =2;
    public int roundsOfGrowthTracker=0;
    public enum GameState
    {
        AugmentSelection,
        Shopping,
        Combat,
        GameOver
    }

    public GameState CurrentState { get; private set; } = GameState.Combat;
    public int CurrentWaveInCycle => currentWaveInCycle;
    public int WavesPerAugmentCycle => wavesPerAugmentCycle;

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
        // Find references if not assigned
        if (augmentSelectionUI == null)
        {
            augmentSelectionUI = FindFirstObjectByType<AugmentSelectionUI>();
        }
        if (shopManager == null)
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }
        if (waveManager == null)
        {
            waveManager = FindFirstObjectByType<WaveManager>();
        }
        if (newRound == null)
        {
            newRound = FindFirstObjectByType<NewRound>();
        }
        if (tileMapManager == null)
        {
            tileMapManager = FindFirstObjectByType<TileMapManager>();
        }
        if (musicController == null)
        {
            musicController = FindFirstObjectByType<MusicController>();
        }
        if (enemyPreviewManager == null)
        {
            enemyPreviewManager = FindFirstObjectByType<EnemyPreviewManager>();
        }
        if (modifierManager == null)
        {
            modifierManager = RoundModifierManager.Instance;
        }
        if (FindFirstObjectByType<GameSpeedButton>() == null)
        {
            GameObject speedButtonObject = new GameObject("GameSpeedButtonController");
            speedButtonObject.AddComponent<GameSpeedButton>();
        }
        if (FindFirstObjectByType<WaveCycleProgressUI>() == null)
        {
            GameObject waveProgressObject = new GameObject("WaveCycleProgressUI");
            waveProgressObject.AddComponent<WaveCycleProgressUI>();
        }

        // Subscribe to augment selection
        if (augmentSelectionUI != null)
        {
            augmentSelectionUI.OnAugmentSelected += OnAugmentSelected;
        }

        // Start the game loop after a frame to ensure all components are initialized
        StartCoroutine(StartGameLoopDelayed());
    }

    protected virtual IEnumerator StartGameLoopDelayed()
    {
        // Wait one frame to ensure all Start() methods have run
        yield return null;

        // Check for duplicate or missing components
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
        SetGameState(GameState.Combat);

        // Start the run in combat after a short pause. Augment selection still happens after a full cycle.
        Debug.Log("Starting first combat phase...");

        // Select and apply modifiers for the very first cycle
        SelectAndApplyModifiers();
        StartCombatPhase();
    }

    protected void ShowAugmentSelection()
    {
        SetGameState(GameState.AugmentSelection);
        Debug.Log("=== AUGMENT SELECTION PHASE ===");

        if (shopManager != null)
        {
            shopManager.SetGameplayUIVisible(false);
            shopManager.ResetRerollCost();
        }
        
        if (augmentSelectionUI != null)
        {
            augmentSelectionUI.ShowAugmentSelection();
        }
    }

    private void OnAugmentSelected(int augmentIndex)
    {
        Debug.Log($"Player selected augment {augmentIndex}. Starting combat phase.");

        // Reset wave cycle counter
        currentWaveInCycle = 0;

        if (FindFirstObjectByType<BoardManager>() is BoardManager boardManager)
        {
            boardManager.RestoreRespawnRoster();
        }

        // Select and apply fresh modifiers for the incoming cycle
        SelectAndApplyModifiers();

        // Move to combat phase
        StartCombatPhase();
    }

    /// <summary>
    /// Selects a new set of random modifiers for the upcoming cycle and applies them.
    /// Protected so subclasses (e.g. a tutorial GameLoopManager variant) can override
    /// to force specific modifiers or skip the system entirely.
    /// </summary>
    protected virtual void SelectAndApplyModifiers()
    {
        // Re-fetch in case it wasn't ready during Start()
        if (modifierManager == null)
            modifierManager = RoundModifierManager.Instance;

        if (modifierManager == null) return;

        modifierManager.SelectModifiersForCycle();
        modifierManager.ApplyModifiers();
    }

    protected void StartShoppingPhase()
    {
        Debug.Log($"Shopping phase skipped. Starting combat for wave {currentWaveInCycle + 1}/{wavesPerAugmentCycle}.");
        StartCombatPhase();
    }

    private IEnumerator AutoStartCombatPhase()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, autoStartRoundDelay));
        if (CurrentState != GameState.Shopping || !isGameActive)
        {
            yield break;
        }

        if (augmentManager != null)
        {
            augmentManager.ApplyAllActiveAugments();
        }

        StartCombatPhase();
    }

    /// <summary>
    /// Called by ShopManager when player presses "Next Round" button
    /// </summary>
    public void OnNextRoundPressed()
    {
        StartCombatPhase();
    }

    private void StartCombatPhase()
    {
        SetGameState(GameState.Combat);
        Debug.Log($"=== COMBAT PHASE - Wave {waveManager.currentWave} (Cycle: {currentWaveInCycle + 1}/{wavesPerAugmentCycle}) ===");

        if (shopManager != null)
        {
            shopManager.SetGameplayUIVisible(true);
        }

        if (GameSpeedButton.Instance != null)
        {
            GameSpeedButton.Instance.SetPaused(false);
            GameSpeedButton.Instance.ResetToDefaultSpeed();
        }

        // Show the enemy preview, then start the wave once it finishes.
        // Keeping this as a void method means all callers (OnAugmentSelected,
        // OnNextRoundPressed, etc.) continue to work without any changes.
        StartCoroutine(StartCombatWithPreview());
    }

    /// <summary>
    /// Runs the enemy preview pan, then starts the wave and begins monitoring it.
    /// Separated from StartCombatPhase() so that method stays a plain void and
    /// all its existing callers remain unchanged.
    /// </summary>
    private IEnumerator StartCombatWithPreview()
    {
        // Wait for the round modifier notification to finish before panning the
        // camera to the enemy preview, so the two never overlap on screen.
        // WaitUntilHidden() returns immediately if no modifier UI is present or
        // if no modifier is active this cycle — so this never causes a hang.
        if (RoundModifierUI.Instance != null)
            yield return StartCoroutine(RoundModifierUI.Instance.WaitUntilHidden());

        // Pan camera to enemy preview. If EnemyPreviewManager is not present
        // in the scene the wave starts immediately with no additional delay.
        if (enemyPreviewManager != null)
            yield return StartCoroutine(enemyPreviewManager.RunPreview());

        if (waveManager != null)
        {
            waveManager.StartNextWave();
        }

        // Monitor wave completion
        StartCoroutine(WaitForWaveCompletion());
    }

    protected virtual IEnumerator WaitForWaveCompletion()
    {
        // Stop waiting if the wave ends normally or if the player runs out of lives.
        yield return new WaitUntil(() => waveManager.IsWaveComplete() || waveManager.IsGameOver());

        if (waveManager.IsGameOver() || waveManager.lives <= 0)
        {
            GameOver();
            yield break;
        }

        Debug.Log("Wave cleared!");

        if (SfxManager.Instance != null)
            SfxManager.Instance.PlayRoundComplete();

        if (newRound != null)
        {
            newRound.startNewRound();
        }

        RefreshShopAfterWaveClear();

        // Increment AFTER completing the wave
        currentWaveInCycle++;
        Debug.Log($"Waves completed in cycle: {currentWaveInCycle}/{wavesPerAugmentCycle}");

        // Check if we've completed the cycle (3 waves)
        if (currentWaveInCycle >= wavesPerAugmentCycle)
        {
            // Return to augment selection
            Debug.Log("Cycle complete! Returning to augment selection...");
            yield return new WaitForSeconds(1f); // Brief pause

            // Remove round modifiers before augment selection so the board is
            // clean when the player makes their augment choice. OnRoundEnd callbacks
            // fire first (reverting direct tower changes), then Context.Reset() runs.
            modifierManager?.RemoveModifiers();

            ShowAugmentSelection();

            

            // if (roundsOfGrowthTracker < roundsOfGrowth)
            // {
            //     roundsOfGrowthTracker++;
            //     if (tileMapManager != null)
            //     {
            //         tileMapManager.expansion(2);
            //     }
            //     else
            //     {
            //         Debug.LogWarning("GameLoopManager: TileMapManager not found, skipping board expansion.");
            //     }
            // }
            
        }
        else
        {

            // Continue directly to the next combat phase.
            Debug.Log($"Moving to next combat phase... (Next: Wave {currentWaveInCycle + 1}/{wavesPerAugmentCycle})");
            yield return new WaitForSeconds(Mathf.Max(0f, autoStartRoundDelay));

            StartCombatPhase();
        }
    }

    // triggers when we run out of lives 
    protected void GameOver()
    {
        SetGameState(GameState.GameOver);
        isGameActive = false;
        Debug.Log("=== GAME OVER ===");
        
        // Wait 5 seconds then load game over scene
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

    protected void RefreshShopAfterWaveClear()
    {
        if (shopManager == null)
        {
            return;
        }

        if (reopenShopEachRound)
        {
            shopManager.OpenShop();
            return;
        }

        shopManager.RefreshShopContents();
    }

    protected void SetGameState(GameState newState)
    {
        CurrentState = newState;

        if (musicController != null)
        {
            musicController.ApplyState(newState);
        }
    }

    private void OnDestroy()
    {
        if (augmentSelectionUI != null)
        {
            augmentSelectionUI.OnAugmentSelected -= OnAugmentSelected;
        }
    }

}
