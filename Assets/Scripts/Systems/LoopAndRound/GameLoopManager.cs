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
    [SerializeField] protected AugmentSelectionUI augmentSelectionUI;
    [SerializeField] protected ShopManager shopManager;
    [SerializeField] protected WaveManager waveManager;
     public AugmentManager augmentManager;

    public NewRound newRound;


    [Header("Settings")]
    [SerializeField] protected int wavesPerAugmentCycle = 3;

    protected int currentWaveInCycle = 0;
    protected bool isGameActive = false;
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
        
        // Show augment selection at game start
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
        
        // Reset wave cycle counter
        currentWaveInCycle = 0;
        
        // Move to shopping phase
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
           waveManager.expandBoard();
            StartShoppingPhase();
        }
    }

    // triggers when we run out of lives 
    protected void GameOver()
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
