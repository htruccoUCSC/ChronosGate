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
     public AugmentManager augmentManager;

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
        
        // Move to combat phase
        StartCombatPhase();
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
