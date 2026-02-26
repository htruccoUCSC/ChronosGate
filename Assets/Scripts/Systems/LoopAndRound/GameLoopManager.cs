using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Main game loop manager that orchestrates the flow:
/// Game Start -> Augment Selection -> 3x(Shop -> Wave) -> Back to Augment Selection
/// </summary>
public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private AugmentSelectionUI augmentSelectionUI;
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private WaveManager waveManager;
     public AugmentManager augmentManager;

    public NewRound newRound;

    [Header("Settings")]
    [SerializeField] private int wavesPerAugmentCycle = 3;

    private int currentWaveInCycle = 0;
    private bool isGameActive = false;
    private bool waitingForNextRound = false;

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

    private void Start()
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

        // Subscribe to augment selection
        if (augmentSelectionUI != null)
        {
            augmentSelectionUI.OnAugmentSelected += OnAugmentSelected;
        }

        // Start the game loop after a frame to ensure all components are initialized
        StartCoroutine(StartGameLoopDelayed());
    }

    private IEnumerator StartGameLoopDelayed()
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
        
        // Show augment selection at game start
        ShowAugmentSelection();
    }

    private void ShowAugmentSelection()
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
        
        // Reset wave cycle counter
        currentWaveInCycle = 0;
        
        // Move to shopping phase
        StartShoppingPhase();
    }

    private void StartShoppingPhase()
    {
        CurrentState = GameState.Shopping;
        Debug.Log($"=== SHOPPING PHASE (Wave {currentWaveInCycle + 1}/{wavesPerAugmentCycle}) ===");
        
        if (shopManager != null)
        {
            shopManager.OpenShop();
        }

        // Wait for player to press "Next Round" button
        waitingForNextRound = true;
    }

    /// <summary>
    /// Called by ShopManager when player presses "Next Round" button
    /// </summary>
    public void OnNextRoundPressed()
    {
        if (!waitingForNextRound) return;
        
        waitingForNextRound = false;
        
        Debug.Log("Next Round pressed. Starting combat phase...");
        augmentManager.ApplyAllActiveAugments();
        StartCombatPhase();
    }

    private void StartCombatPhase()
    {
        CurrentState = GameState.Combat;
        Debug.Log($"=== COMBAT PHASE - Wave {waveManager.currentWave} (Cycle: {currentWaveInCycle + 1}/{wavesPerAugmentCycle}) ===");
        
        if (waveManager != null)
        {
            waveManager.StartNextWave();
        }

        // Monitor wave completion
        StartCoroutine(WaitForWaveCompletion());
    }

    private IEnumerator WaitForWaveCompletion()
    {
        // Wait until all enemies are cleared
        yield return new WaitUntil(() => waveManager.IsWaveComplete());

        Debug.Log("Wave cleared!");

        // Check if game is over (no lives left)
        if (waveManager.lives <= 0)
        {
            GameOver();
            yield break;
        }

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
        }
        else
        {

            // Continue to next shop phase. Wave done
            Debug.Log($"Moving to next shopping phase... (Next: Wave {currentWaveInCycle + 1}/{wavesPerAugmentCycle})");
            yield return new WaitForSeconds(1f); // Brief pause
            newRound.startNewRound();
            StartShoppingPhase();
        }
    }

    // triggers when we run out of lives 
    private void GameOver()
    {
        CurrentState = GameState.GameOver;
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

    private void OnDestroy()
    {
        if (augmentSelectionUI != null)
        {
            augmentSelectionUI.OnAugmentSelected -= OnAugmentSelected;
        }
    }
}
